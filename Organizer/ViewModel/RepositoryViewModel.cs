// Copyright (C) 2011, Dmitry Ignatiev <lovesan.ru at gmail.com>

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT.  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE

using System;
using System.Collections.Generic;
using System.Linq;
using Organizer.Model;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;

namespace Organizer.ViewModel
{
    public class RepositoryViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        TaskRepository _repository;
        String _filename;
        IDialogService _dialog;
        WorkspaceCollectionViewModel<CollectionTreeViewModel> _tree;
        WorkspaceCollectionViewModel<WorkspaceViewModel> _workspaces;
        RelayCommand _newCollectionCommand;
        RelayCommand _saveCommand;
        RelayCommand _saveAsCommand;

        public RepositoryViewModel(TaskRepository repository, IDialogServiceFactory fileDialogServiceFactory, String filename)
        {           
            if (null == repository)
                throw new ArgumentNullException("repository");
            if (null == fileDialogServiceFactory)
                throw new ArgumentNullException("fileDialogService");
            _dialog = fileDialogServiceFactory.CreateService();
            _repository = repository;
            _filename = filename;
            _workspaces = new WorkspaceCollectionViewModel<WorkspaceViewModel>();
            _tree = new WorkspaceCollectionViewModel<CollectionTreeViewModel>(
                _repository.Select(x => new CollectionTreeViewModel(x, _repository, _dialog)));
            _tree.PropertyChanged += this.OnChildCollectionPropertyChanged;
            _tree.RequestOpen += this.OnWorkspaceRequestOpen;
            _workspaces.PropertyChanged += this.OnChildCollectionPropertyChanged;
            _workspaces.RequestOpen += this.OnWorkspaceRequestOpen;
            _workspaces.RequestClose += this.OnWorkspaceRequestClose;

            _repository.RepositoryChanged += this.OnRepositoryChanged;
            this.IsSaved = true;
        }

        public RepositoryViewModel(TaskRepository repository, IDialogServiceFactory fileDialogServiceFactory)
            : this(repository, fileDialogServiceFactory, null)
        {
        }

        void OnChildCollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
            if ("IsSaved" == e.PropertyName && !(sender as WorkspaceViewModel).IsSaved)
                this.IsSaved = false;
        }

        void OnRepositoryChanged(TaskRepository repository, RepositoryChangedEventArgs e)
        {
            if (!e.Removed)
                _tree.Add(new CollectionTreeViewModel(e.Element, _repository, _dialog));
        }

        void OnWorkspaceRequestOpen(object sender, WorkspaceRequestEventArgs e)
        {
            _workspaces.Add(e.Workspace);
            var cv = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (null != cv)
                cv.MoveCurrentTo(e.Workspace);
        }

        void OnWorkspaceRequestClose(object sender, WorkspaceRequestEventArgs e)
        {
            _workspaces.Remove(e.Workspace);
        }

        string IDataErrorInfo.Error
        {
            get { return (_repository as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error;
                switch (propertyName)
                {
                    case "Name":
                        error = (_repository as IDataErrorInfo)[propertyName];
                        break;
                    case "DisplayName":
                    case "Collections":
                    case "Workspaces":
                    case "Filename":
                    case "IsSaved":
                        error = null;
                        break;
                    default:
                        throw new ArgumentException("Invalid property name: " + propertyName, "propertyName");
                }
                CommandManager.InvalidateRequerySuggested();
                return error;
            }
        }

        void AddCollection()
        {
            var c = new TaskCollection();
            _repository.Add(c);
            this.IsSaved = false;
        }

        public ICommand NewCollectionCommand
        {
            get
            {
                if (null == _newCollectionCommand)
                    _newCollectionCommand = new RelayCommand(param => this.AddCollection());
                return _newCollectionCommand;
            }
        }
        
        protected override void OnRequestClose(object sender, WorkspaceRequestEventArgs e)
        {
            if (this.IsSaved)
                base.OnRequestClose(sender, e);
            else
            {
                bool? r = _dialog.MessageBox(
                    "Changes have been made to repository. " +
                    "Are you sure you want to close it?",
                    "Repository hasn't been saved.",
                    MessageBoxDialogButtons.OkCancel,
                    MessageBoxDialogIcon.Question);
                if (r.HasValue && r.Value)
                    base.OnRequestClose(sender, e);
            }
        }

        bool CanSave()
        {
            return _repository.IsValid;
        }

        void Save()
        {            
            if (null == _filename)
                _filename = _dialog.SaveFileDialog("Repository files|*.xml|All files|*.*");
            if (null != _filename)
            {
                _repository.Save(_filename);
                string error = (_repository as IDataErrorInfo).Error;
                if (null != error)
                    _dialog.MessageBox(error, "Error", MessageBoxDialogButtons.Ok, MessageBoxDialogIcon.Error);
                else
                    this.IsSaved = true;
            }
        }

        void SaveAs()
        {
            String filename = _filename;
            _filename = null;
            this.Save();
            if(null == _filename)
                _filename = filename;
        }

        public ICommand SaveCommand
        {
            get 
            {
                if (null == _saveCommand)
                    _saveCommand = new RelayCommand(param => this.Save(), param => this.CanSave());
                return _saveCommand;
            }
        }

        public ICommand SaveAsCommand
        {
            get
            {
                if (null == _saveAsCommand)
                    _saveAsCommand = new RelayCommand(param => this.SaveAs(), param => this.CanSave());
                return _saveAsCommand;
            }
        }

        public String Filename
        {
            get 
            {
                return _filename;
            }
            set
            {
                if (value == _filename)
                    return;
                _filename = value;
                base.OnPropertyChanged("Filename");
            }
        }

        public String Name
        {
            get { return _repository.Name; }
            set
            {
                _repository.Name = value;
                base.OnPropertyChanged("Name");
                this.IsSaved = false;
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Repository<" + this.Name + ">";
            }
        }

        public WorkspaceCollectionViewModel<CollectionTreeViewModel> Collections
        {
            get { return _tree; }
        }

        public WorkspaceCollectionViewModel<WorkspaceViewModel> Workspaces
        {
            get { return _workspaces; }
        }

        protected override void OnDispose()
        {
            _dialog.Dispose();
            _repository.RepositoryChanged -= this.OnRepositoryChanged;
            _tree.PropertyChanged -= this.OnChildCollectionPropertyChanged;
            _tree.RequestOpen -= this.OnWorkspaceRequestOpen;
            _workspaces.PropertyChanged -= this.OnChildCollectionPropertyChanged;
            _workspaces.RequestOpen -= this.OnWorkspaceRequestOpen;
            _workspaces.RequestClose -= this.OnWorkspaceRequestClose;
            _workspaces.Dispose();
            _tree.Dispose();
            base.OnDispose();
        }
    }
}
