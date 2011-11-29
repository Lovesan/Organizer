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
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using System.Reflection;
using System.Collections;

namespace Organizer.Model
{
    public class TaskRepository : ICollection<TaskCollection>, IDataErrorInfo
    {
        string _error;
        List<TaskCollection> _collections;        

        public TaskRepository(String name)
        {
            this.Name = name;
            _error = null;
            _collections = new List<TaskCollection>();
        }

        public TaskRepository() : this(null) { }

        public void Save(string filename)
        {
            if (!this.IsValid)
                throw new InvalidOperationException("Unable to save invalid repository");
            _error = null;
            
            XmlDocument doc = new XmlDocument();            
            ValidationEventHandler handler = (sender, e) => { throw e.Exception; };
            using (var s = Assembly.GetCallingAssembly().GetManifestResourceStream("Organizer.TaskRepositorySchema.xsd"))
            {
                doc.Schemas.Add(XmlSchema.Read(s, handler));
            }
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement node = doc.CreateElement(null, "TaskRepository", "http://localhost/TaskRepositorySchema");
            node.Attributes.Append(doc.CreateAttribute("Name"));
            node.SetAttribute("Name", Name);
            foreach (var c in _collections)
            {
                XmlElement child = node.OwnerDocument.CreateElement(null, "TaskCollection", "http://localhost/TaskRepositorySchema");
                node.AppendChild(child);
                c.Save(child);                
            }
            doc.AppendChild(node);
            try
            {
                doc.Save(filename);
            }
            catch (Exception e)
            {
                _error = (null != e.Message) ? e.Message : "Unable to save repository.";
            }
        }

        public static TaskRepository Load(string filename)
        {
            TaskRepository r = new TaskRepository();
            r._collections = new List<TaskCollection>();

            ValidationEventHandler handler = (sender, e) =>
            {
                throw e.Exception;
            };

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CheckCharacters = true;
            settings.CloseInput = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = false;
            settings.IgnoreWhitespace = true;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.None;
            settings.ValidationEventHandler += handler;
            using (var s = Assembly.GetCallingAssembly().GetManifestResourceStream("Organizer.TaskRepositorySchema.xsd"))
            {
                settings.Schemas.Add(XmlSchema.Read(s, handler));
            }

            try
            {
                using (var xmlReader = XmlReader.Create(filename, settings))
                {
                    var doc = new XmlDocument();
                    doc.Load(xmlReader);
                    XmlNode repoNode = doc.SelectSingleNode("*");
                    r.Name = repoNode.Attributes["Name"].Value;
                    foreach (XmlNode node in repoNode.ChildNodes)
                    {
                        r._collections.Add(TaskCollection.Load(node, null));
                    }
                }
            }
            catch (Exception e)
            {
                r._error = (null != e.Message) ? e.Message : "Unable to load repository.";
                r._collections.Clear();
            }

            return r;
        }
        
        public String Name { get; set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Name)
                       && _collections.TrueForAll(tc => tc.IsValid);
            }
        }

        string IDataErrorInfo.Error
        {
            get { return _error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case "Name":
                        return String.IsNullOrWhiteSpace(Name) ? "Invalid repository name." : null;
                    case "IsValid":
                    case "Count":
                    case "IsReadOnly":
                        return null;
                    default:
                        throw new ArgumentException("Unknown property name.", "propertyName");
                }
            }
        }

        public event RepositoryChangedEventHandler RepositoryChanged;

        void OnRepositoryChanged(TaskCollection item, bool removed)
        {
            var handler = this.RepositoryChanged;
            if (null != handler)
                handler(this, new RepositoryChangedEventArgs(item, removed));
        }

        public void Add(TaskCollection item)
        {
            if (null == item)
                throw new ArgumentNullException("item");
            item.Collection = null;
            if (!_collections.Contains(item))
            {
                _collections.Add(item);
                this.OnRepositoryChanged(item, false);
            }
        }

        public void Clear()
        {
            while(_collections.Count > 0)
            {
                var item = _collections[0];
                item.Collection = null;
                _collections.Remove(item);
                this.OnRepositoryChanged(item, true);
            }
        }

        public bool Contains(TaskCollection item)
        {
            return _collections.Contains(item);
        }

        public void CopyTo(TaskCollection[] array, int arrayIndex)
        {
            foreach (var c in _collections)
            {
                array[arrayIndex] = c;
                ++arrayIndex;
            }
        }

        public int Count
        {
            get { return _collections.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TaskCollection item)
        {
            if (null == item)
                throw new ArgumentNullException("item");
            if (!_collections.Contains(item))
                return false;
            item.Collection = null;
            _collections.Remove(item);
            this.OnRepositoryChanged(item, true);
            return true;
        }

        public IEnumerator<TaskCollection> GetEnumerator()
        {
            return _collections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collections.GetEnumerator();
        }
    }
}
