// 
// ImportAddinFileAttribute.cs
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
	/// <summary>
	/// Declares an add-in file import
	/// </summary>
	/// <remarks>
	/// An add-in may be composed by several assemblies and data files.
	/// Data files must be declared in the main assembly using this attribute, or in the XML manifest.
	/// 
	/// It is important to properly declare all files used by an add-in. 
	/// This information is used by setup tools to know exactly what needs to be packaged when creating 
	/// an add-in package, or to know what needs to be deleted when removing an add-in.
	/// </remarks>
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
	public class ImportAddinFileAttribute: Attribute
	{
		string filePath;
		
		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="filePath">
		/// Path to the file. Must be relative to the assembly declaring this attribute.
		/// </param>
		public ImportAddinFileAttribute (string filePath)
		{
			this.filePath = filePath;
		}
		
		/// <summary>
		/// Path to the file. Must be relative to the assembly declaring this attribute.
		/// </summary>
		public string FilePath {
			get { return filePath; }
			set { filePath = value; }
		}
	}
}

