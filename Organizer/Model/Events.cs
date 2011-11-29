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
using System.Text;

namespace Organizer.Model
{
    public delegate void RepositoryChangedEventHandler(TaskRepository repository, RepositoryChangedEventArgs e);

    public class RepositoryChangedEventArgs : EventArgs
    {
        public RepositoryChangedEventArgs(TaskCollection element, bool removed)
        {
            this.Element = element;
            this.Removed = removed;
        }

        public TaskCollection Element { get; private set; }
        public bool Removed { get; private set; }
    }

    public delegate void CollectionChangedEventHandler(TaskCollection collection, CollectionChangedEventArgs e);

    public class CollectionChangedEventArgs : EventArgs
    {
        public CollectionChangedEventArgs(CollectionElement element, bool removed)
        {
            this.Element = element;
            this.Removed = removed;
        }

        public CollectionElement Element { get; private set; }
        public bool Removed { get; private set; }
    }

    public delegate void ParentChangedEventHandler(CollectionElement element, ParentChangedEventArgs e);

    public class ParentChangedEventArgs : EventArgs
    {
        public ParentChangedEventArgs(TaskCollection from, TaskCollection to)
        {
            this.From = from;
            this.To = to;
        }

        public TaskCollection From { get; private set; }
        public TaskCollection To { get; private set; }
    }
}
