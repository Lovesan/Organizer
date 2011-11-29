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
using System.Windows.Input;

namespace Organizer.ViewModel
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        bool _saved;
        RelayCommand _openCommand;
        RelayCommand _closeCommand;
        RelayCommand _deleteCommand;

        protected WorkspaceViewModel()
        {
            _saved = true;
        }

        public ICommand OpenCommand
        {
            get
            {
                if (null == _openCommand)
                    _openCommand = new RelayCommand(param => this.OnRequestOpen(this, new WorkspaceRequestEventArgs(this)));
                return _openCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (null == _closeCommand)
                    _closeCommand = new RelayCommand(param => this.OnRequestClose(this, new WorkspaceRequestEventArgs(this)));
                return _closeCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if (null == _deleteCommand)
                    _deleteCommand = new RelayCommand(param => this.OnRequestDelete(this, new WorkspaceRequestEventArgs(this)));
                return _deleteCommand;
            }
        }

        public event WorkspaceRequestEventHandler RequestOpen;
        public event WorkspaceRequestEventHandler RequestClose;
        public event WorkspaceRequestEventHandler RequestDelete;

        protected virtual void OnRequestOpen(object sender, WorkspaceRequestEventArgs e)
        {
            var handler = this.RequestOpen;
            if (null != handler)
                handler(sender, e);
        }

        protected virtual void OnRequestClose(object sender, WorkspaceRequestEventArgs e)
        {
            var handler = this.RequestClose;
            if (null != handler)
                handler(sender, e);
        }

        protected virtual void OnRequestDelete(object sender, WorkspaceRequestEventArgs e)
        {
            var handler = this.RequestDelete;
            if (null != handler)
                handler(sender, e);
        }

        public virtual bool IsSaved
        {
            get { return _saved; }
            protected set
            {
                _saved = value;
                this.OnPropertyChanged("IsSaved");
            }
        }
    }
}
