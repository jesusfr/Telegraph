// 
// ExtensionAttributeAttribute.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace Mono.Addins
{
	public class ExtensionAttributeAttribute: Attribute
	{
		Type targetType;
		string targetTypeName;
		string name;
		string val;
		string path;
		
		public ExtensionAttributeAttribute (string name, string value)
		{
			Name = name;
			Value = value;
		}
		
		public ExtensionAttributeAttribute (Type type, string name, string value)
		{
			Name = name;
			Value = value;
			Type = type;
		}
		
		public ExtensionAttributeAttribute (string path, string name, string value)
		{
			Name = name;
			Value = value;
			Path = path;
		}
		
		public string Name {
			get { return this.name; }
			set { this.name = value; }
		}

		public string Value {
			get { return this.val; }
			set { this.val = value; }
		}

		public string Path {
			get { return this.path; }
			set { this.path = value; }
		}
		
		public Type Type {
			get { return targetType; }
			set { targetType = value; targetTypeName = targetType.FullName; }
		}
		
		internal string TypeName {
			get { return targetTypeName ?? string.Empty; }
			set { targetTypeName = value; }
		}
	}
}

