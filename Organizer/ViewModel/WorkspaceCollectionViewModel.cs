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
using System.Collections;
using System.ComponentModel;

namespace Organizer.ViewModel
{
    public class WorkspaceCollectionViewModel <T> : WorkspaceViewModel, ICollection<T>, INotifyCollectionChanged
        where T : WorkspaceViewModel
    {
        List<T> _workspaces;

        public WorkspaceCollectionViewModel()
        {           
            _workspaces = new List<T>();
        }

        public WorkspaceCollectionViewModel(IEnumerable<T> workspaces)
        {
            if (null == workspaces)
                throw new ArgumentNullException("workspaces");
            _workspaces = new List<T>(workspaces);
            this.OnCollectionChanged(_workspaces, false, 0);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        void OnItemChanged(T ws, bool removed, int index)
        {
            if (removed)
            {
                ws.PropertyChanged -= this.OnItemPropertyChanged;
                ws.RequestOpen -= base.OnRequestOpen;
                ws.RequestClose -= base.OnRequestClose;
                ws.RequestDelete -= this.OnItemRequestDelete;                                
            }
            else
            {
                ws.PropertyChanged += this.OnItemPropertyChanged;
                ws.RequestOpen += base.OnRequestOpen;
                ws.RequestClose += base.OnRequestClose;
                ws.RequestDelete += this.OnItemRequestDelete;
            }
            var handler = this.CollectionChanged;
            if (null != handler)
            {
                var e = new NotifyCollectionChangedEventArgs(
                    removed ? NotifyCollectionChangedAction.Remove : NotifyCollectionChangedAction.Add,
                    ws,
                    index);
                handler(this, e);
            }
        }

        void OnCollectionChanged(List<T> items, bool removed, int index)
        {
            if (removed)
            {
                foreach (var ws in items)
                {
                    ws.PropertyChanged -= this.OnItemPropertyChanged;
                    ws.RequestOpen -= base.OnRequestOpen;
                    ws.RequestClose -= base.OnRequestClose;
                    ws.RequestDelete -= this.OnItemRequestDelete;
                }
            }
            else
            {
                foreach (var ws in items)
                {
                    ws.PropertyChanged += this.OnItemPropertyChanged;
                    ws.RequestOpen += base.OnRequestOpen;
                    ws.RequestClose += base.OnRequestClose;
                    ws.RequestDelete += this.OnItemRequestDelete;
                }
            }
            var handler = this.CollectionChanged;
            if (null != handler)
            {
                var e = new NotifyCollectionChangedEventArgs(
                    removed ? NotifyCollectionChangedAction.Remove : NotifyCollectionChangedAction.Add,
                    items,
                    index);
                handler(this, e);
            }
        }
        
        void OnItemRequestDelete(object sender, WorkspaceRequestEventArgs e)
        {
            this.Remove(e.Workspace as T);
        }

        void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (("IsSaved" == e.PropertyName) && !(sender as WorkspaceViewModel).IsSaved)
                this.IsSaved = false;
        }

        public void Add(T item)
        {
            _workspaces.Add(item);
            OnItemChanged(item, false, _workspaces.Count-1);
        }

        public void Clear()
        {
            var items = new List<T>(_workspaces);
            _workspaces.Clear();
            OnCollectionChanged(items, true, 0);
        }

        public bool Contains(T item)
        {
            return _workspaces.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach(var ws in _workspaces)
            {
                array[arrayIndex] = ws;
                ++arrayIndex;
            }
        }

        public int Count
        {
            get { return _workspaces.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int index = _workspaces.IndexOf(item);
            if (index >= 0)
            {                
                this.OnItemChanged(item, true, index);
                _workspaces.Remove(item);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _workspaces.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _workspaces.GetEnumerator();
        }

        protected override void OnDispose()
        {
            foreach (var ws in _workspaces)
            {
                ws.PropertyChanged -= this.OnItemPropertyChanged;
                ws.RequestOpen -= base.OnRequestOpen;
                ws.RequestClose -= base.OnRequestClose;
                ws.RequestDelete -= this.OnItemRequestDelete;
                ws.Dispose();
            }
            _workspaces.Clear();
            base.OnDispose();
        }
    }
}
