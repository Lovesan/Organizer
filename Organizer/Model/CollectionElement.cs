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
using System.Xml;

namespace Organizer.Model
{
    public abstract class CollectionElement
    {
        TaskCollection _collection;

        protected CollectionElement() { }

        public event ParentChangedEventHandler ParentChanged;
        
        public TaskCollection Collection
        {
            get { return _collection; }
            set
            {
                if (value == _collection)
                    return;
                if (null != _collection)
                    _collection.InternalRemove(this);
                if (null != value)
                    value.InternalAdd(this);
                var oldCollection = _collection;
                _collection = value;
                var handler = ParentChanged;
                if (null != handler)
                {
                    handler(this, new ParentChangedEventArgs(oldCollection, _collection));
                }
            }
        }

        internal abstract void Save(XmlElement node);

        public abstract bool IsValid { get; }
    }
}
