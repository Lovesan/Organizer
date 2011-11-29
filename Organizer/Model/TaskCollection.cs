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
using System.ComponentModel;
using System.Xml;
using System.Collections;

namespace Organizer.Model
{
    public class TaskCollection : CollectionElement, ICollection<CollectionElement>, IDataErrorInfo
    {
        List<CollectionElement> _elements;

        public TaskCollection(String name)
        {
            this.Name = name;
            _elements = new List<CollectionElement>();
        }

        public TaskCollection() : this(null) { }

        internal override void Save(XmlElement node)
        {
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("Name"));
            node.SetAttribute("Name", Name);
            foreach (CollectionElement element in _elements)
            {
                XmlElement child = node.OwnerDocument.CreateElement(null, element.GetType().Name, "http://localhost/TaskRepositorySchema");
                node.AppendChild(child);
                element.Save(child);                
            }
        }

        internal static TaskCollection Load(XmlNode node, TaskCollection collection)
        {
            TaskCollection c = new TaskCollection();
            c.Collection = collection;
            c.Name = node.Attributes["Name"].Value;
            foreach(XmlNode childNode in node.ChildNodes)
            {
                if ("TaskCollection" == childNode.LocalName)
                {                    
                    TaskCollection.Load(childNode, c);
                }
                else
                {
                    Task.Load(childNode, c);
                }
            }
            return c;
        }
        
        public String Name { get; set; }

        public override bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Name)
                       && _elements.TrueForAll(x => x.IsValid);
            }
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case "Name":
                        return String.IsNullOrWhiteSpace(Name) ? "Invalid task collection name." : null;
                    case "IsValid":
                    case "Tasks":
                    case "Children":
                    case "Count":
                    case "IsReadOnly":
                        return null;
                    default:
                        throw new ArgumentException("Unknown property name.", "propertyName");
                }
            }
        }

        public event CollectionChangedEventHandler CollectionChanged;

        internal void InternalAdd(CollectionElement element)
        {
            _elements.Add(element);
            var handler = this.CollectionChanged;
            if (null != handler)
                handler(this, new CollectionChangedEventArgs(element, false));
                        
        }

        internal void InternalRemove(CollectionElement element)
        {
            _elements.Remove(element);
            var handler = this.CollectionChanged;
            if (null != handler)
                handler(this, new CollectionChangedEventArgs(element, true));
        }


        public void Add(CollectionElement item)
        {
            if (null == item)
                throw new ArgumentNullException("item");
            item.Collection = this;
        }

        public void Clear()
        {
            while (_elements.Count > 0)
            {
                _elements[0].Collection = null;
            }
        }

        public bool Contains(CollectionElement item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(CollectionElement[] array, int arrayIndex)
        {
            foreach (var element in _elements)
            {
                array[arrayIndex] = element;
                ++arrayIndex;
            }
        }

        public int Count
        {
            get { return _elements.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CollectionElement item)
        {
            if (null == item)
                throw new ArgumentNullException("item");
            if (this != item.Collection)
                return false;
            item.Collection = null;
            return true;
        }

        public IEnumerator<CollectionElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
