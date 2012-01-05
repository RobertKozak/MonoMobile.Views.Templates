//
// UIGlassyButton.cs: 
//
// Author:
//   Robert Kozak (rkozak@gmail.com / Twitter:@robertkozak)
//
// Copyright 2011, Nowcom Corporation
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
	using MonoTouch.CoreAnimation;
	using MonoTouch.CoreGraphics;
	using MonoTouch.Foundation;
	using MonoTouch.UIKit;

	public class UIGlassyButton : UIButton
	{
		private bool _Initialized;
		private NSTimer _Timer;
		private string _Caption = string.Empty;
		private CAGradientLayer _HighlightLayer;
		
		public float CornerRadius { get; set; }

		public UIColor ButtonTintColor { get; set; }
		public UIColor TextColor { get; set; }
		public UIColor HighlightColor { get; set; }

		public string Caption
		{ 
			get { return _Caption; } 
			set { if (_Caption != value) { _Caption = value; SetTitle(Caption, UIControlState.Normal); } }
		}
				
		public UIGlassyButton(RectangleF rect, string title): this(rect, title, UIColor.FromRGB(11, 71, 167), UIColor.White)
		{
		}

		public UIGlassyButton(RectangleF rect, string title, UIColor tintColor, UIColor textColor): base(rect)
		{
			Font = UIFont.BoldSystemFontOfSize(12);

			CornerRadius = 6;
			
			ButtonTintColor = UIColor.FromRGB(11, 71, 167);
			HighlightColor = UIColor.Black;
			TextColor = textColor;

			_Caption = title;
		}

		protected override void Dispose(bool disposing)
		{
			if (TextColor != null)
			{
				TextColor.Dispose();
				TextColor = null;
			}

			if (HighlightColor != null)
			{
				HighlightColor.Dispose();
				HighlightColor = null;
			}
			
			if (ButtonTintColor != null)
			{
				ButtonTintColor.Dispose();
				ButtonTintColor = null;
			}

			if (_HighlightLayer != null)
			{
				_HighlightLayer.RemoveFromSuperLayer();
				_HighlightLayer.Dispose();
			}

			if (Font != null)
			{
				Font.Dispose();
			}

			base.Dispose(disposing);
		}
		
		public void Initialize(RectangleF rect)
		{
			Layer.MasksToBounds = true;
			Layer.CornerRadius = CornerRadius;

			VerticalAlignment = UIControlContentVerticalAlignment.Center;
			
			var gradientFrame = rect;
			
			var shineFrame = gradientFrame;
			shineFrame.Y += 1;
			shineFrame.X += 1;
			shineFrame.Width -= 2;
			shineFrame.Height = (shineFrame.Height / 2);

			var shineLayer = new CAGradientLayer();
			shineLayer.Frame = shineFrame;
			shineLayer.Colors = new MonoTouch.CoreGraphics.CGColor[] { UIColor.White.ColorWithAlpha(0.60f).CGColor, UIColor.White.ColorWithAlpha(0.10f).CGColor };
			shineLayer.CornerRadius = CornerRadius - 1.5f;
			
			var backgroundLayer = new CAGradientLayer();
			backgroundLayer.Frame = gradientFrame;
			backgroundLayer.Colors = new MonoTouch.CoreGraphics.CGColor[] { ButtonTintColor.CGColor, ButtonTintColor.CGColor };
			
			_HighlightLayer = new CAGradientLayer();
			_HighlightLayer.Frame = gradientFrame;
			_HighlightLayer.Colors = new MonoTouch.CoreGraphics.CGColor[] { HighlightColor.ColorWithAlpha(0.10f).CGColor, HighlightColor.ColorWithAlpha(0.60f).CGColor };	

			Layer.AddSublayer(backgroundLayer);
			Layer.AddSublayer(shineLayer);
			Layer.AddSublayer(_HighlightLayer);

			SetTitle(Caption, UIControlState.Normal);
			SetTitleColor(TextColor, UIControlState.Normal);
			
			_Initialized = true;
		}

		public override void Draw(RectangleF rect)
		{
			base.Draw(rect);

			if (!_Initialized)
			{
				Initialize(rect);
			}
			
			UIView.BeginAnimations(null);
			UIView.SetAnimationDuration(0.20f);
				
			if (Highlighted)
			{
				_HighlightLayer.Opacity = 100f;
			}
			else
			{
				_HighlightLayer.Opacity = 0f;
			}

			UIView.CommitAnimations();
		}
		
		public override bool BeginTracking(UITouch uitouch, UIEvent uievent)
		{
			if (uievent.Type == UIEventType.Touches)
			{
				SetNeedsDisplay();
			}

			base.BeginTracking(uitouch, uievent);
			return true;
		}

		public override void EndTracking(UITouch uitouch, UIEvent uievent)
		{
			if (uievent.Type == UIEventType.Touches)
			{
				_Timer = NSTimer.CreateScheduledTimer(TimeSpan.FromMilliseconds(400), ButtonTimer);
				SetNeedsDisplay();
			}

			base.EndTracking(uitouch, uievent);
		}
		
		public override void SetTitle(string title, UIControlState forState)
		{
			if (title != null)
			{
				base.SetTitle(title, forState);

				SetTitleShadowColor(HighlightColor.ColorWithAlpha(0.40f), UIControlState.Normal);
				TitleShadowOffset = new SizeF(0, -1);
	
				var textLayer = Layer.Sublayers[0];
				Layer.AddSublayer(textLayer);
			}
		}

		private void ButtonTimer()
		{
			if (_Timer != null)
			{
				_Timer.Invalidate();
				_Timer = null;
			}

			Highlighted = false;
			SetNeedsDisplay();
		}
	}
}
