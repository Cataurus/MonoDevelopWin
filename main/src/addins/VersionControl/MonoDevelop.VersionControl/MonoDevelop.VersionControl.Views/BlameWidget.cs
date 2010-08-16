// 
// BlameWidget.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
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
using System.Linq;
using Gtk;
using Gdk;
using System.Collections.Generic;
using Mono.TextEditor;
using MonoDevelop.Ide;
using System.Threading;
using MonoDevelop.Core;
using System.Text.RegularExpressions;
using System.Text;

namespace MonoDevelop.VersionControl.Views
{
	public class BlameWidget : Bin
	{
		Adjustment vAdjustment;
		Gtk.VScrollbar vScrollBar;
		
		Adjustment hAdjustment;
		Gtk.HScrollbar hScrollBar;
		
		BlameRenderer overview;
		
		TextEditor editor;
		List<ContainerChild> children = new List<ContainerChild> ();
		
		public Adjustment Vadjustment {
			get { return this.vAdjustment; }
		}

		public Adjustment Hadjustment {
			get { return this.hAdjustment; }
		}
		
		public override ContainerChild this [Widget w] {
			get {
				foreach (ContainerChild info in children.ToArray ()) {
					if (info.Child == w)
						return info;
				}
				return null;
			}
		}
		
		public TextEditor Editor {
			get {
				return this.editor;
			}
		}
		VersionControlDocumentInfo info;
		
		public Ide.Gui.Document Document {
			get {
				return info.Document;
			}
		}
		public VersionControlItem VersionControlItem {
			get {
				return info.Item;
			}
		}

		protected BlameWidget (IntPtr ptr) : base (ptr)
		{
		}

		public BlameWidget (VersionControlDocumentInfo info)
		{
			this.info = info;
			
			vAdjustment = new Adjustment (0, 0, 0, 0, 0, 0);
			vAdjustment.Changed += HandleAdjustmentChanged;
			
			vScrollBar = new VScrollbar (vAdjustment);
			AddChild (vScrollBar);
			
			hAdjustment = new Adjustment (0, 0, 0, 0, 0, 0);
			hAdjustment.Changed += HandleAdjustmentChanged;
			
			hScrollBar = new HScrollbar (hAdjustment);
			AddChild (hScrollBar);
			
			editor = new TextEditor (info.Document.Editor.Document, info.Document.Editor.Options);
			AddChild (editor);
			editor.SetScrollAdjustments (hAdjustment, vAdjustment);
			
			overview = new BlameRenderer (this);
			AddChild (overview);
			
			this.DoubleBuffered = true;
			editor.ExposeEvent += HandleEditorExposeEvent;
			editor.EditorOptionsChanged += delegate {
				overview.OptionsChanged ();
			};
			editor.Caret.PositionChanged += ComparisonWidget.CaretPositionChanged;
			editor.FocusInEvent += ComparisonWidget.EditorFocusIn;
			editor.Document.Folded += delegate {
				QueueDraw ();
			};
			editor.Document.FoldTreeUpdated += delegate {
				QueueDraw ();
			};
			Show ();
		}

		void HandleAdjustmentChanged (object sender, EventArgs e)
		{
			Adjustment adjustment = (Adjustment)sender;
			Scrollbar scrollbar = adjustment == vAdjustment ? (Scrollbar)vScrollBar : hScrollBar;
			bool newVisible = adjustment.Upper - adjustment.Lower > adjustment.PageSize;
			if (scrollbar.Visible != newVisible) {
				scrollbar.Visible = newVisible;
				QueueResize ();
			}
		}
		
		public override GLib.GType ChildType ()
		{
			return Gtk.Widget.GType;
		}
		
		protected override void ForAll (bool include_internals, Gtk.Callback callback)
		{
			base.ForAll (include_internals, callback);
			
			if (include_internals)
				children.ForEach (child => callback (child.Child));
		}
		
		public void AddChild (Gtk.Widget child)
		{
			child.Parent = this;
			children.Add (new ContainerChild (this, child));
			child.Show ();
		}
		
		protected override void OnAdded (Widget widget)
		{
			base.OnAdded (widget);
			if (widget == Child)
				widget.SetScrollAdjustments (hAdjustment, vAdjustment);
		}
		
		protected override void OnRemoved (Widget widget)
		{
			widget.Unparent ();
			foreach (var info in children.ToArray ()) {
				if (info.Child == widget) {
					children.Remove (info);
					break;
				}
			}
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			children.ForEach (child => child.Child.Destroy ());
			children.Clear ();
		}
		 
		protected override void OnSizeAllocated (Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			int vwidth = vScrollBar.Visible ? vScrollBar.Requisition.Width : 0;
			int hheight = hScrollBar.Visible ? hScrollBar.Requisition.Height : 0; 
			Rectangle childRectangle = new Rectangle (allocation.X + 1, allocation.Y + 1, allocation.Width - vwidth - 1, allocation.Height - hheight - 1);
			
			if (vScrollBar.Visible) {
				int right = childRectangle.Right;
				int vChildTopHeight = -1;
				int v = hScrollBar.Visible ? hScrollBar.Requisition.Height : 0;
				vScrollBar.SizeAllocate (new Rectangle (right, childRectangle.Y + vChildTopHeight, vwidth, Allocation.Height - v - vChildTopHeight - 1));
				vScrollBar.Value = System.Math.Max (System.Math.Min (vAdjustment.Upper - vAdjustment.PageSize, vScrollBar.Value), vAdjustment.Lower);
			}
			int overviewWidth = overview.WidthRequest;
			overview.SizeAllocate (new Rectangle (childRectangle.Right - overviewWidth, childRectangle.Top, overviewWidth, childRectangle.Height));
			editor.SizeAllocate (new Rectangle (childRectangle.X, childRectangle.Top, childRectangle.Width - overviewWidth, childRectangle.Height));
		
			if (hScrollBar.Visible) {
				hScrollBar.SizeAllocate (new Rectangle (childRectangle.X, childRectangle.Bottom, childRectangle.Width, hheight));
				hScrollBar.Value = System.Math.Max (System.Math.Min (hAdjustment.Upper - hAdjustment.PageSize, hScrollBar.Value), hAdjustment.Lower);
			}
		}
		
		static double GetWheelDelta (Scrollbar scrollbar, ScrollDirection direction)
		{
			double delta = System.Math.Pow (scrollbar.Adjustment.PageSize, 2.0 / 3.0);
			if (direction == ScrollDirection.Up || direction == ScrollDirection.Left)
				delta = -delta;
			if (scrollbar.Inverted)
				delta = -delta;
			return delta;
		}
		
		protected override bool OnScrollEvent (EventScroll evnt)
		{
			Scrollbar scrollWidget = (evnt.Direction == ScrollDirection.Up || evnt.Direction == ScrollDirection.Down) ? (Scrollbar)vScrollBar : hScrollBar;
			if (scrollWidget.Visible) {
				double newValue = scrollWidget.Adjustment.Value + GetWheelDelta (scrollWidget, evnt.Direction);
				newValue = System.Math.Max (System.Math.Min (scrollWidget.Adjustment.Upper  - scrollWidget.Adjustment.PageSize, newValue), scrollWidget.Adjustment.Lower);
				scrollWidget.Adjustment.Value = newValue;
			}
			return base.OnScrollEvent (evnt);
		}
		
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);
			children.ForEach (child => child.Child.SizeRequest ());
		}
		
		void HandleEditorExposeEvent (object o, ExposeEventArgs args)
		{
			using (Cairo.Context cr = Gdk.CairoHelper.Create (args.Event.Window)) {
				cr.LineWidth = Math.Max (1.0, Editor.Options.Zoom);
				int startLine = Editor.YToLine (Editor.VAdjustment.Value);
				double startY = Editor.LineToY (startLine);
				double curY = startY - Editor.VAdjustment.Value;
				int line = startLine;
				JumpOverFoldings (ref line);
				while (curY < editor.Allocation.Bottom) {
					Annotation ann = line < overview.annotations.Count ? overview.annotations[line] : null;
					
					do {
						double lineHeight = Editor.GetLineHeight (line);
						curY += lineHeight;
						line++;
						JumpOverFoldings (ref  line);
					} while (line + 1 < overview.annotations.Count && ann != null && overview.annotations[line] != null && overview.annotations[line].Revision == ann.Revision);
					
					if (ann != null) {
						cr.MoveTo (Editor.TextViewMargin.XOffset, curY + 0.5);
						cr.LineTo (Editor.Allocation.Width, curY + 0.5);
						var color = Style.Dark (State);
						cr.Color = new Cairo.Color (color.Red / (double)ushort.MaxValue, 
						                            color.Green / (double)ushort.MaxValue,
						                            color.Blue / (double)ushort.MaxValue,
						                            0.2);
						cr.Stroke ();
					}
				}
			}
		}
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Gdk.GC gc = Style.DarkGC (State);
			evnt.Window.DrawLine (gc, Allocation.X, Allocation.Top, Allocation.X, Allocation.Bottom);
			evnt.Window.DrawLine (gc, Allocation.Right, Allocation.Top, Allocation.Right, Allocation.Bottom);
			
			evnt.Window.DrawLine (gc, Allocation.Left, Allocation.Y, Allocation.Right, Allocation.Y);
			evnt.Window.DrawLine (gc, Allocation.Left, Allocation.Bottom, Allocation.Right, Allocation.Bottom);
			
			
			return base.OnExposeEvent (evnt);
		}
		
		void JumpOverFoldings (ref int line)
		{
			int lastFold = -1;
			foreach (FoldSegment fs in Editor.Document.GetStartFoldings (line).Where (fs => fs.IsFolded)) {
				lastFold = System.Math.Max (fs.EndOffset, lastFold);
			}
			if (lastFold > 0) 
				line = Editor.Document.OffsetToLineNumber (lastFold);
		}

		class BlameRenderer : DrawingArea 
		{
			static readonly Annotation locallyModified = new Annotation ("", "?", DateTime.MinValue);
			
			BlameWidget widget;
			internal List<Annotation> annotations;
			Pango.Layout layout;

			double dragPosition = -1;
			public BlameRenderer (BlameWidget widget)
			{
				this.widget = widget;
				widget.info.Updated += delegate { QueueDraw (); };
				annotations = new List<Annotation> ();
				UpdateAnnotations (null, null);
	//			widget.Document.Saved += UpdateAnnotations;
				widget.Editor.Document.TextReplacing += EditorDocumentTextReplacing;
				widget.Editor.Document.LineChanged += EditorDocumentLineChanged;
				widget.vScrollBar.ValueChanged += delegate {
					QueueDraw ();
				};
				
				layout = new Pango.Layout (PangoContext);
				Events |= EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.ButtonMotionMask;
				OptionsChanged ();
				Show ();
			}
			
			public void OptionsChanged ()
			{
				var description = Pango.FontDescription.FromString ("Tahoma " + (int)(10 * widget.Editor.Options.Zoom));
				layout.FontDescription = description;
				UpdateWidth ();
			}
			
			protected override void OnDestroyed ()
			{
				base.OnDestroyed ();
//				widget.Document.Saved -= UpdateAnnotations;
				widget.Editor.Document.TextReplacing -= EditorDocumentTextReplacing;
				widget.Editor.Document.LineChanged -= EditorDocumentLineChanged;
				if (layout != null) {
					layout.Dispose ();
					layout = null;
				}
			}
			
			protected override bool OnMotionNotifyEvent (EventMotion evnt)
			{
				if (dragPosition >= 0) {
					int x, y;
					widget.GetPointer (out x, out y);
					int newWidthRequest = widget.Allocation.Width - x;
					newWidthRequest = Math.Min (widget.Allocation.Width - (int)widget.Editor.TextViewMargin.XOffset, Math.Max (leftSpacer, newWidthRequest));
					
					WidthRequest = newWidthRequest;
					QueueResize ();
				}
				return base.OnMotionNotifyEvent (evnt);
			}
			uint grabTime;
			protected override bool OnButtonPressEvent (EventButton evnt)
			{
				if (evnt.X < leftSpacer) {
					grabTime = evnt.Time;
					var status = Gdk.Pointer.Grab (this.GdkWindow, false, EventMask.PointerMotionHintMask | EventMask.Button1MotionMask | EventMask.ButtonReleaseMask | EventMask.EnterNotifyMask | EventMask.LeaveNotifyMask, null, null, grabTime);
					if (status == GrabStatus.Success) {
						dragPosition = evnt.X;
					}
				}
				return base.OnButtonPressEvent (evnt);
			}
			
			protected override bool OnButtonReleaseEvent (EventButton evnt)
			{
				if (dragPosition >= 0) {
					Gdk.Pointer.Ungrab (grabTime);
					dragPosition = -1;
				}
				return base.OnButtonReleaseEvent (evnt);
			}
			
			/// <summary>
			/// Reloads annotations for the current document
			/// </summary>
			internal void UpdateAnnotations (object sender, EventArgs e)
			{
				StatusBarContext ctx = IdeApp.Workbench.StatusBar.CreateContext ();
				ctx.AutoPulse = true;
				ctx.ShowMessage (ImageService.GetImage ("md-version-control", IconSize.Menu), GettextCatalog.GetString ("Retrieving history"));
				
				ThreadPool.QueueUserWorkItem (delegate {
					try {
						annotations = new List<Annotation> (widget.VersionControlItem.Repository.GetAnnotations (widget.Document.FileName));
					} catch (Exception ex) {
						LoggingService.LogError ("Error retrieving history", ex);
					}
					
					DispatchService.GuiDispatch (delegate {
						ctx.Dispose ();
						UpdateWidth ();
						QueueDraw ();
					});
				});
			}
	
			/// <summary>
			/// Marks a line as locally modified
			/// </summary>
			private void EditorDocumentLineChanged (object sender, LineEventArgs e)
			{
				int startLine = widget.Editor.Document.OffsetToLineNumber (e.Line.Offset);
				SetAnnotation (startLine, locallyModified);
			}
			
			/// <summary>
			/// Marks necessary lines modified when text is replaced
			/// </summary>
			private void EditorDocumentTextReplacing (object sender, ReplaceEventArgs e)
			{
				int  startLine = widget.Editor.Document.OffsetToLineNumber (e.Offset),
				     endLine = widget.Editor.Document.OffsetToLineNumber (e.Offset + Math.Max (e.Count, e.Value != null ? e.Value.Length : 0)),
				     lineCount = 0;
				string[] tokens = null;
				
				if (startLine < endLine) {
					// change crosses line boundary
					
					lineCount = endLine - startLine;
					lineCount = Math.Min (lineCount, annotations.Count - startLine);
					
					if (lineCount > 0)
						annotations.RemoveRange (startLine, lineCount);
					if (!string.IsNullOrEmpty (e.Value)) {
						for (int i=0; i<lineCount; ++i)
							annotations.Insert (startLine, locallyModified);
					}
					return;
				} else if (0 == e.Count) {
						// insert
						tokens = e.Value.Split (new string[]{Environment.NewLine}, StringSplitOptions.None);
						lineCount = tokens.Length - 1;
						for (int i=0; i<lineCount; ++i) {
							annotations.Insert (Math.Min (startLine + 1, annotations.Count), locallyModified);
						}
				} else if (startLine > endLine) {
					// revert
					UpdateAnnotations (null, null);
					return;
				}
				
				SetAnnotation (startLine, locallyModified);
			}
			
			void SetAnnotation (int index, Annotation text)
			{
				if (index < 0)
					return;
				for (int i = annotations.Count; i <= index; ++i)
					annotations.Add (locallyModified);
				annotations[index] = text;
			}
	
			/// <summary>
			/// Gets the commit message matching a given annotation index.
			/// </summary>
			internal string GetCommitMessage (int index)
			{
				Annotation annotation = (index < annotations.Count)? annotations[index]: null;
				var history = widget.info.History;
				if (null != history && annotation != null) {
					foreach (Revision rev in history) {
						if (rev.ToString () == annotation.Revision) {
							return rev.Message;
						}
					}
				}
				return null;
			}

			void UpdateWidth ()
			{
				int tmpwidth, height, width = 120;
				int dateTimeLength = -1;
				foreach (Annotation note in annotations) {
					if (!string.IsNullOrEmpty (note.Author)) { 
						if (dateTimeLength < 0 && note.HasDate) {
							layout.SetText (note.Date.ToShortDateString ());
							layout.GetPixelSize (out dateTimeLength, out height);
						}
						layout.SetText (note.Author + note.Revision);
						layout.GetPixelSize (out tmpwidth, out height);
						width = Math.Max (width, tmpwidth);
					}
				}
				WidthRequest = dateTimeLength + width + 30 + leftSpacer + margin * 2;
				QueueResize ();
			}

			Regex regex = new Regex (@"[\s+\*[^:]*\:\s*]*(?<msg>[^:\s*](.)*)$");
			const int leftSpacer = 4;
			const int margin = 4;

			protected override bool OnExposeEvent (Gdk.EventExpose e)
			{
				using (Cairo.Context cr = Gdk.CairoHelper.Create (e.Window)) {
					cr.LineWidth = Math.Max (1.0, widget.Editor.Options.Zoom);
					
					cr.Rectangle (leftSpacer, 0, Allocation.Width, Allocation.Height);
					cr.Color = new Cairo.Color (0.95, 0.95, 0.95);
					cr.Fill ();
					
					int startLine = widget.Editor.YToLine ((int)widget.Editor.VAdjustment.Value);
					double startY = widget.Editor.LineToY (startLine);
					
					while (startLine > 0 && startLine < annotations.Count && annotations[startLine - 1] != null && annotations[startLine] != null && annotations[startLine - 1].Revision == annotations[startLine].Revision) {
						startLine--;
						startY -= widget.Editor.GetLineHeight (widget.Editor.Document.GetLine (startLine));
					}
					
					double curY = startY - widget.Editor.VAdjustment.Value;
					int line = startLine;
					while (curY < Allocation.Bottom) {
						double curStart = curY;
						widget.JumpOverFoldings (ref line);
						int lineStart = line;
						int w = 0, w2 = 0, h = 16;
						Annotation ann = line < annotations.Count ? annotations[line] : null;
						if (ann != null) {
							layout.SetText (ann.Author);
							layout.GetPixelSize (out w, out h);
							e.Window.DrawLayout (Style.BlackGC, leftSpacer + margin, (int)(curY + (widget.Editor.LineHeight - h) / 2), layout);
							
							
							layout.SetText (ann.Revision);
							layout.GetPixelSize (out w2, out h);
							e.Window.DrawLayout (Style.BlackGC, Allocation.Width - w2 - margin, (int)(curY + (widget.Editor.LineHeight - h) / 2), layout);

							if (ann.HasDate) {
								string dateTime = ann.Date.ToShortDateString ();
								int middle = w + (Allocation.Width - margin * 2 - leftSpacer - w - w2) / 2;
								layout.SetText (dateTime);
								layout.GetPixelSize (out w, out h);
								e.Window.DrawLayout (Style.BlackGC, leftSpacer + margin + middle - w / 2, (int)(curY + (widget.Editor.LineHeight - h) / 2), layout);
							}
						
							do {
								double lineHeight = widget.Editor.GetLineHeight (line);
								curY += lineHeight;
								line++;
								widget.JumpOverFoldings (ref line);
							} while (line + 1 < annotations.Count && annotations[line] != null && annotations[line].Revision == ann.Revision);
						} else {
							curY += widget.Editor.GetLineHeight (line);
							line++;
							widget.JumpOverFoldings (ref line);
						}
						
						if (ann != null && line - lineStart > 1) {
							string msg = GetCommitMessage (lineStart);
							if (!string.IsNullOrEmpty (msg)) {
								StringBuilder sb = new StringBuilder ();
								bool wasWs = false;
								foreach (char ch in msg) {
									if (char.IsWhiteSpace (ch)) {
										if (!wasWs)
											sb.Append (' ');
										wasWs = true;
										continue;
									}
									wasWs = false;
									sb.Append (ch);
								}
								var match = regex.Match (sb.ToString ());
								if (match.Success)
									msg = match.Result ("${msg}").Trim ();
								layout.SetText (msg);
								layout.Width = (int)(Allocation.Width * Pango.Scale.PangoScale);
								using (var gc = new Gdk.GC (e.Window)) {
									gc.RgbFgColor = Style.Dark (State);
									gc.ClipRectangle = new Rectangle (0, (int)curStart, Allocation.Width, (int)(curY - curStart));
									e.Window.DrawLayout (gc, (int)(leftSpacer + margin), (int)(curStart + h), layout);
								}
							}
						}
						
						cr.Rectangle (0, curStart, leftSpacer, curY - curStart);
						
						if (ann != null && ann != locallyModified && !string.IsNullOrEmpty (ann.Author)) {
							
							double a = 1.0;
							var history = widget.info.History;
							if (ann != null && history != null) {
								for (int i = 0; i < history.Length; i++) {
									Revision rev = history[i];
									if (rev.ToString () == ann.Revision) {
										a = i / (double)history.Length;
										break;
									}
								}
							} else {
								a = 1;
							}
	
							HslColor color = new Cairo.Color (0.90, 0.90, 1);
							color.L = 0.4 + a / 2;
							color.S = 1 - a / 2;
							cr.Color = color;
						} else {
							cr.Color = ann != null ? new Cairo.Color (1, 1, 0) : new Cairo.Color (0.95, 0.95, 0.95);
						}
						cr.Fill ();

						if (ann != null) {
							cr.MoveTo (0, curY + 0.5);
							cr.LineTo (Allocation.Width, curY + 0.5);
							cr.Color = new Cairo.Color (0.6, 0.6, 0.6);
							cr.Stroke ();
						}
					}
				}
				return true;
			}
			
		}	
	}
}

