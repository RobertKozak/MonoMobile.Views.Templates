//
// AccessoryButtonAttribute.cs:
//
// Author:
//   Robert Kozak (rkozak@gmail.com / Twitter:@robertkozak
//
// Copyright 2011, Nowcom Corportation
//
// Code licensed under the MIT X11 license
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace MonoMobile.Views.Templates
{
	using System;
	using System.Drawing;
	using System.Reflection;
	using MonoTouch.Foundation;
	using MonoTouch.UIKit;
	
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, Inherited = false)]
	public class AccessoryButtonAttribute : ButtonAttribute
	{
		public override Type CellViewType { get { return typeof(AccessoryButtonCellView); } }
	
		public AccessoryButtonAttribute(): this(null)
		{
		}
	
		public AccessoryButtonAttribute(string commandMemberName): base(commandMemberName)
		{
		}
		
		public string Title { get; set; }
		public string ButtonTintColor { get; set; }
		public string TextColor { get; set; }

		[Preserve(AllMembers = true)]
		class AccessoryButtonCellView : ButtonCellView
		{
			private AccessoryButtonAttribute AccessoryButtonData { get { return CellViewTemplate as AccessoryButtonAttribute; } }
	
			private UIButton _AccessoryButton;

			public AccessoryButtonCellView(RectangleF frame) : base(frame)
			{
			}

			public override void Initialize()
			{
				base.Initialize();

				_AccessoryButton = CreateButton();
			}

			protected override void Dispose(bool disposing)
			{
				if (_AccessoryButton != null)
				{			
					_AccessoryButton.TouchUpInside -= ButtonTouched;

					_AccessoryButton.Dispose();
					_AccessoryButton = null;
				}

				base.Dispose(disposing);
			}

			public override void UpdateCell(UITableViewCell cell, NSIndexPath indexPath)
			{
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				if (AccessoryButtonData.Command.CanExecute(AccessoryButtonData.CommandParameter))
				{
					cell.AccessoryView = _AccessoryButton;
				}
				else
				{
					cell.AccessoryView = null;
				}
			}

			protected UIButton CreateButton()
			{
				UIColor buttonTintColor = UIColor.FromRGB(12, 45, 158);
				UIColor textColor = UIColor.White;

				if (!string.IsNullOrEmpty(AccessoryButtonData.ButtonTintColor))
				{
					buttonTintColor = AccessoryButtonData.ButtonTintColor.ToUIColor(); 
				}

				if (!string.IsNullOrEmpty(AccessoryButtonData.TextColor))
				{
					textColor = AccessoryButtonData.TextColor.ToUIColor(); 
				}

				var title = string.Empty;
				if (!string.IsNullOrEmpty(AccessoryButtonData.Title))
				{
					title = AccessoryButtonData.Title;
				}

				var size = StringSize(title, UIFont.BoldSystemFontOfSize(UIFont.ButtonFontSize));
				var width = size.Width < 38 ? 38 : size.Width - 10;
 
				var button = new UIGlassyButton(new RectangleF(0, 0, width, 28), title, buttonTintColor, textColor);
				button.TouchUpInside += ButtonTouched;

				return button;
			}
			
			private void ButtonTouched(object sender, EventArgs e)
			{
				AccessoryButtonData.Command.Execute(AccessoryButtonData.CommandParameter);
			}
		}
	}
}

