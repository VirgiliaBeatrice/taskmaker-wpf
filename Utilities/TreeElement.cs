﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using taskmaker_wpf.Views;
using taskmaker_wpf.Views.Widgets;
using System.Windows;

namespace taskmaker_wpf.Utilities {
    public interface ITreeElement {
        string Name { get; set; }
        ITreeElement Parent { get; set; }

        void Add(ITreeElement elem);
        void Remove(ITreeElement elem);
    }

    public class TreeElement : DependencyObject {
        public string Name { get; set; } = "";
        public TreeElement Parent { get; set; } = null;
        protected virtual List<TreeElement> Children { get; set; } = new List<TreeElement>();

        public object Data { get; set; }

        public TreeElement() { }

        public TreeElement(TreeElement other) {
            Name = other.Name;
            Data = other.Data;
        }

        public void AddChild(TreeElement child) {
            Children.Add(child);
            child.Parent = this;
        }

        public bool RemoveChild(TreeElement child) {
            return Children.Remove(child);
        }

        public bool Remove(string name) {
            var target = Children.Where(e => e.Name == name).First();

            return Children.Remove(target);
        }

        public void RemoveType<T>() where T: TreeElement {
            Children.OfType<T>()
                .ToList()
                .ForEach(e => Children.Remove(e));
        }

        public void RemoveAll() {
            // Free unmanaged resources
            foreach (var e in Children) {
                if (e is IDisposable disposable) {
                    disposable.Dispose();
                }
            }

            Children.Clear();
        }

        public List<TreeElement> GetAllChild() {
            return Children;
        }

        public IEnumerable<TreeElement> Flatten() {
            var collection = new List<TreeElement>() { this };

            foreach(var child in Children) {
                collection.AddRange(child.Flatten());
            }

            return collection;
        }

        public TreeElement FindByName(string name) {
            return Flatten().Where(e => e.Name == name).FirstOrDefault();
        }

        public T FindByName<T>(string name) where T : class {
            return FindByName(name) as T;
        }

        public string PrintAllChild() {
            var content = new StringBuilder();
            content.AppendLine($"|-- {Name}");

            foreach (var child in Children) {
                var output = child.PrintAllChild();

                using (var stream = new MemoryStream()) {

                    var writer = new StreamWriter(stream);

                    writer.Write(output);
                    writer.Flush();
                    stream.Position = 0;

                    var sr = new StreamReader(stream);
                    var line = sr.ReadLine();

                    content.Append(' ', 4);
                    content.AppendLine(line);

                    while (!sr.EndOfStream) {
                        line = sr.ReadLine();

                        content.Append(' ', 4);
                        content.AppendLine(line);
                    }
                }
            }

            return content.ToString();
        }

        public TreeElement GetRoot() {
            var root = Parent;

            while (root.Parent != null) {
                root = root.Parent;
            }

            return root;
        }

        static public TreeElement Clone(TreeElement node) {
            var newNode = new TreeElement(node);

            foreach (var child in node.Children) {
                var newChildNode = Clone(child);

                newNode.AddChild(newChildNode);
            }

            return newNode;
        }

        public TreeElement Clone() {
            return Clone(this);
        }
    }
}
