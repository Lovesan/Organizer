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
using System.Security;

namespace Organizer.Model
{
    public class Task : CollectionElement, IDataErrorInfo
    {
        public Task(String name, bool isCompleted, DateTime dateTime, String text)
        {
            this.Name = name;
            this.IsCompleted = isCompleted;            
            this.DateTime = dateTime;
            this.Text = text;
        }

        public Task(String name, DateTime dateTime, String text)
            : this(name, false, dateTime, text) { }
        
        public Task() : this(null, false, DateTime.Now, "") { }
        
        internal override void Save(XmlElement node)
        {
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("Name"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("IsCompleted"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("DateTime"));
            node.SetAttribute("Name", Name);
            node.SetAttribute("IsCompleted", XmlConvert.ToString(IsCompleted));
            node.SetAttribute("DateTime", XmlConvert.ToString(DateTime, XmlDateTimeSerializationMode.RoundtripKind));
            node.AppendChild(node.OwnerDocument.CreateTextNode(Text));            
        }
        
        internal static void Load(XmlNode node, TaskCollection collection)
        {
            var task = new Task(
                node.Attributes["Name"].Value,
                XmlConvert.ToBoolean(node.Attributes["IsCompleted"].Value),
                XmlConvert.ToDateTime(node.Attributes["DateTime"].Value, XmlDateTimeSerializationMode.RoundtripKind),
                node.InnerText);
            task.Collection = collection;
        }
        
        public String Name { get; set; }
        public bool IsCompleted{get;set;}
        public DateTime DateTime { get; set; }
        public String Text { get; set; }
        public override bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Name);
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
                        return String.IsNullOrWhiteSpace(Name) ? "Invalid task name." : null;
                    case "IsCompleted":
                    case "DateTime":
                    case "Text":
                    case "IsValid":
                        return null;
                    default:
                        throw new ArgumentException("Invalid property name: " + propertyName, "propertyName");
                }
            }
        }
    }
}
