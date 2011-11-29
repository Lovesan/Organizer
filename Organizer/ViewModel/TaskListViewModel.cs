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
using System.Collections.Specialized;
using System.Linq;
using Organizer.Model;
using System.Windows.Input;
using System.ComponentModel;

namespace Organizer.ViewModel
{
    public class TaskListViewModel : WorkspaceCollectionViewModel<TaskViewModel>, IDataErrorInfo
    {
        TaskCollection _taskCollection;
        TaskRepository _repository;
        RelayCommand _newTaskCommand;
        CollectionTreeViewModel _parentTree;
        IDialogService _dialog;

        public TaskListViewModel(TaskCollection taskCollection, TaskRepository repository, IDialogService dialogService, CollectionTreeViewModel parentTree)
            : base(taskCollection
                   .Where(x => x is Task)
                   .Select(x => new TaskViewModel(x as Task, dialogService)))
        {
            if (null == repository)
                throw new ArgumentNullException("repository");
            _dialog = dialogService;
            _repository = repository;
            _taskCollection = taskCollection;
            _taskCollection.CollectionChanged += OnTaskCollectionChanged;
            _parentTree = parentTree;
            if (null != _parentTree)
            {
                parentTree.PropertyChanged += OnParentPropertyChanged;
            }
        }

        public TaskListViewModel(TaskCollection taskCollection, TaskRepository repository, IDialogService dialogService)
            : this(taskCollection, repository, dialogService, null)
        {
        }

        void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("DisplayName" == e.PropertyName)
                base.OnPropertyChanged("DisplayName");
        }

        void OnTaskCollectionChanged(TaskCollection c, CollectionChangedEventArgs e)
        {
            if (e.Removed || !(e.Element is Task))
                return;
            var tvm = new TaskViewModel(e.Element as Task, _dialog);
            this.Add(tvm);
        }

        protected override void OnRequestOpen(object sender, WorkspaceRequestEventArgs e)
        {
            if(this != e.Workspace)
                base.OnRequestOpen(sender, e);
        }

        protected override void OnRequestDelete(object sender, WorkspaceRequestEventArgs e)
        {
            this.IsSaved = false;
            base.OnRequestClose(this, new WorkspaceRequestEventArgs(this));
            if (null == _taskCollection.Collection)
                _repository.Remove(_taskCollection);
            else
                _taskCollection.Collection = null;
        }

        void NewTask()
        {
            var t = new Task();
            _taskCollection.Add(t);            
            this.IsSaved = false;
            var tvm = new TaskViewModel(t, _dialog);
            base.OnRequestOpen(tvm, new WorkspaceRequestEventArgs(tvm));
        }

        public ICommand NewTaskCommand
        {
            get
            {
                if (null == _newTaskCommand)
                    _newTaskCommand = new RelayCommand(param => this.NewTask());
                return _newTaskCommand;
            }
        }

        protected override void OnDispose()
        {
            _taskCollection.CollectionChanged -= this.OnTaskCollectionChanged;
            if (null != _parentTree)
                _parentTree.PropertyChanged -= OnParentPropertyChanged;
            base.OnDispose();
        }

        public override string DisplayName
        {
            get { return _taskCollection.Name; }
        }

        string IDataErrorInfo.Error
        {
            get { return (_taskCollection as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get 
            {
                string error;
                switch(propertyName)
                {
                    case "DisplayName":
                        error = (_taskCollection as IDataErrorInfo)["Name"];
                        break;
                    default:
                        throw new ArgumentException("Invalid property name: " + propertyName, "propertyName");
                }
                return error;
            }
        }
    }
}
