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
using System.ComponentModel;
using System.Windows.Input;
using Organizer.Model;
using System.Text.RegularExpressions;

namespace Organizer.ViewModel
{
    public class TaskViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        Task _task;
        Regex _timeRegex;
        String _timeString;
        IDialogService _dialog;

        internal TaskViewModel(Task task, IDialogService dialogService)
        {
            if (null == task)
                throw new ArgumentNullException("task");
            if (null == dialogService)
                throw new ArgumentNullException("dialogService");

            _dialog = dialogService;
            _task = task;
            _timeRegex = new Regex("[0-2][0-9][:][0-5][0-9]:[0-5][0-9]");
            _task.ParentChanged += OnParentChanged;
        }

        protected override void OnRequestOpen(object sender, WorkspaceRequestEventArgs e)
        {
            e = new WorkspaceRequestEventArgs(new TaskViewModel(_task, _dialog));
            base.OnRequestOpen(this, e);
        }
        
        void OnParentChanged(CollectionElement element, ParentChangedEventArgs e)
        {
            if(e.From != null)
                base.OnRequestDelete(this, new WorkspaceRequestEventArgs(this));
        }

        protected override void OnRequestDelete(object sender, WorkspaceRequestEventArgs e)
        {
            bool? r = _dialog.MessageBox(
                "Are you sure you want to delete task '" + this.DisplayName + "'?",
                "Confirm task deletion.",
                MessageBoxDialogButtons.OkCancel,
                MessageBoxDialogIcon.Question);
            if (r.HasValue && r.Value)
            {
                this.IsSaved = false;
                base.OnRequestClose(this, new WorkspaceRequestEventArgs(this));
                _task.Collection = null;
            }
        }

        string IDataErrorInfo.Error
        {
            get { return (_task as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error;
                switch (propertyName)
                {
                    case "Time":
                        error = ValidateTime();
                        break;
                    case "Date":
                    case "IsCompleted":
                    case "Text":
                    case "IsSaved":
                        error = null;
                        break;
                    case "Name":
                    case "DisplayName":
                        error = (_task as IDataErrorInfo)["Name"];
                        break;
                    default:
                        throw new ArgumentException("Invalid property name: " + propertyName, "propertyName");
                }

                CommandManager.InvalidateRequerySuggested();
                return error;
            }
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
            get { return _task.Name; }
            set
            {
                if (value == _task.Name)
                    return;
                _task.Name = value;
                base.OnPropertyChanged("Name");
                base.OnPropertyChanged("DisplayName");
                this.IsSaved = false;
            }
        }

        public bool IsCompleted
        {
            get { return _task.IsCompleted; }
            set
            {
                if (value == _task.IsCompleted)
                    return;
                _task.IsCompleted = value;
                base.OnPropertyChanged("IsCompleted");
                this.IsSaved = false;
            }
        }

        public DateTime Date
        {
            get { return _task.DateTime.Date; }
            set
            {
                if (value == _task.DateTime)
                    return;
                _task.DateTime = value.Date;
                base.OnPropertyChanged("Date");
                this.IsSaved = false;
            }
        }

        public String Time
        {
            get { return _task.DateTime.ToString("HH:mm:ss"); }
            set
            {
                DateTime time;
                _timeString = value;
                if (TryParseTime(_timeString, out time))
                {
                    DateTime date = _task.DateTime;
                    _task.DateTime = new DateTime(date.Year, date.Month, date.Day,
                                                  time.Hour, time.Minute, time.Second);
                }
                base.OnPropertyChanged("Time");
                this.IsSaved = false;
            }
        }

        bool TryParseTime(String str, out DateTime value)
        {
            value = DateTime.FromFileTimeUtc(0);
            if (String.IsNullOrWhiteSpace(str))
                return false;
            if (!_timeRegex.Match(str).Success)
                return false;
            if (!DateTime.TryParse(str, out value))
                return false;
            return true;
        }

        string ValidateTime()
        {
            DateTime time;
            if ((null != _timeString) && !TryParseTime(_timeString, out time))
                return "Invalid time format.";
            else
                return null;
        }

        public String Text
        {
            get { return _task.Text; }
            set
            {
                if (value == _task.Text)
                    return;
                _task.Text = value;
                base.OnPropertyChanged("Text");
                this.IsSaved = false;
            }
        }
    }
}