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
using System.Linq;
using Organizer.Model;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;

namespace Organizer.ViewModel
{
    public class CollectionTreeViewModel : WorkspaceCollectionViewModel<CollectionTreeViewModel>, IDataErrorInfo
    {
        bool _expanded;
        bool _empty;
        TaskRepository _repository;
        TaskCollection _collection;
        IDialogService _dialog;
        RelayCommand _newCollectionCommand;

        public CollectionTreeViewModel(TaskCollection taskCollection, TaskRepository repository, IDialogService dialogService)
        {
            if (null == "taskCollection")
                throw new ArgumentNullException("taskCollection");
            if (null == repository)
                throw new ArgumentNullException("repository");
            if (null == dialogService)
                throw new ArgumentNullException("dialogService");
            _dialog = dialogService;
            _expanded = false;
            _repository = repository;
            _collection = taskCollection;
            _collection.CollectionChanged += this.OnCollectionChanged;
            _collection.ParentChanged += this.OnParentChanged;
            _repository.RepositoryChanged += this.OnRepositoryChanged;
            _empty = 0 == _collection.Count(x => x is TaskCollection);
        }

        void OnRepositoryChanged(TaskRepository repository, RepositoryChangedEventArgs e)
        {
            if (e.Removed && (e.Element == _collection))
                base.OnRequestDelete(e.Element, new WorkspaceRequestEventArgs(this));
        }

        void OnCollectionChanged(TaskCollection collection, CollectionChangedEventArgs e)
        {
            if (e.Element is TaskCollection)
            {
                if (e.Removed)
                {
                    if (0 == _collection.Count(x => x is TaskCollection))
                    {
                        _empty = true;
                        base.OnPropertyChanged("IsEmpty");
                    }
                }
                else
                {
                    if (_expanded)
                    {
                        this.Add(new CollectionTreeViewModel(e.Element as TaskCollection, _repository, _dialog));
                    }
                    if (_empty)
                    {
                        _empty = false;
                        base.OnPropertyChanged("IsEmpty");
                    }
                }
            }
        }

        void OnParentChanged(CollectionElement element, ParentChangedEventArgs e)
        {
            if(null != e.From)
                base.OnRequestDelete(this, new WorkspaceRequestEventArgs(this));
        }

        protected override void OnRequestDelete(object sender, WorkspaceRequestEventArgs e)
        {
            if (_collection.Count > 0)
            {
                bool? r = _dialog.MessageBox(
                    "Are you sure you want to delete it?",
                    "Task collection contains items.",
                    MessageBoxDialogButtons.OkCancel,
                    MessageBoxDialogIcon.Question);
                if (!r.HasValue || !r.Value)
                    return;
            }
            this.IsSaved = false;
            if (null == _collection.Collection)
                _repository.Remove(_collection);
            else
                _collection.Collection = null;
        }

        protected override void OnRequestClose(object sender, WorkspaceRequestEventArgs e)
        {
            this.IsExpanded = false;
        }

        void AddCollection()
        {
            var c = new TaskCollection();
            _collection.Add(c);
            this.IsSaved = false;
            this.IsExpanded = true;
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
                
        public bool IsExpanded
        {
            get { return _expanded; }
            set
            {
                if (value == _expanded)
                    return;
                _expanded = value;
                if (_expanded)
                {
                    foreach (var c in _collection)
                        if (c is TaskCollection)
                            this.Add(new CollectionTreeViewModel(c as TaskCollection, _repository, _dialog));
                }
                else
                {
                    this.Clear();
                }                
                base.OnPropertyChanged("IsExpanded");
            }
        }

        public bool IsEmpty
        {
            get { return _empty; }
        }

        public override string DisplayName
        {
            get
            {
                return this.Name;
            }
        }

        public String Name
        {
            get { return _collection.Name; }
            set
            {
                if (value == _collection.Name)
                    return;
                _collection.Name = value;
                base.OnPropertyChanged("Name");
                base.OnPropertyChanged("DisplayName");
                this.IsSaved = false;
            }
        }

        string IDataErrorInfo.Error
        {
            get { return (_collection as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error;
                switch (propertyName)
                {
                    case "Name":
                    case "DisplayName":
                        error = (_collection as IDataErrorInfo)["Name"];
                        break;
                    case "IsExpanded":
                    case "IsSaved":
                    case "IsEmpty":
                        error = null;
                        break;
                    default:
                        throw new ArgumentException("Invalid property name: " + propertyName, "propertyName");
                }
                CommandManager.InvalidateRequerySuggested();
                return error;
            }
        }

        protected override void OnRequestOpen(object sender, WorkspaceRequestEventArgs e)
        {
            if (this == e.Workspace)
            {
                var tlvm = new TaskListViewModel(_collection, _repository, _dialog, this);
                e = new WorkspaceRequestEventArgs(tlvm);
            }
            base.OnRequestOpen(sender, e);
        }

        protected override void OnDispose()
        {
            _collection.CollectionChanged -= this.OnCollectionChanged;
            _collection.ParentChanged -= this.OnParentChanged;
            base.OnDispose();
        }
    }
}
