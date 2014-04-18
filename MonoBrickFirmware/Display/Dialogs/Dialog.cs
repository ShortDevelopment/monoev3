using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.UserInput;

namespace MonoBrickFirmware.Display.Dialogs
{
    public abstract class Dialog
	{
		
		protected int numberOfLines;
		protected Lcd lcd;
		protected Font font;
		protected Rectangle dialogWindowOuther; 
		protected Rectangle dialogWindowInner;
		protected Buttons btns;
				
		private string title;
        private List<Rectangle> lines;
        
		private Rectangle titleRect;
		private Point bottomLineCenter;
        
        private int titleSize;
		private const int dialogEdge = 5;
		private int dialogWidth;
		private int dialogHeight;
		private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
		private CancellationToken token;
		
		
		private const int buttonEdge = 2;
		private const int buttonTextOffset = 2;
		private const int boxMiddleOffset = 8;
		
		public Action OnShow = delegate {lcd.SaveScreen();};
		public Action OnExit = delegate {lcd.LoadScreen();};
		
		public Dialog (Font f, Lcd lcd, Buttons btns, string title, int width = 160, int height = 90, int topOffset = 0)
		{
			dialogWidth = width;
			dialogHeight = height;
			this.font = f;
			this.lcd = lcd;
			this.title = title;
			this.btns = btns;
			int xEdge = (Lcd.Width - dialogWidth)/2;
			int yEdge = (Lcd.Height - dialogHeight)/2;
			Point startPoint1 = new Point (xEdge, yEdge);
			Point startPoint2 = new Point (xEdge + dialogWidth, yEdge + dialogHeight);
			this.titleSize = font.TextSize (this.title).X + (int)f.maxWidth;
			dialogWindowOuther = new Rectangle (startPoint1, startPoint2);
			dialogWindowInner = new Rectangle (new Point (startPoint1.X + dialogEdge, startPoint1.Y + dialogEdge), new Point (startPoint2.X - dialogEdge, startPoint2.Y - dialogEdge));
			titleRect = new Rectangle (new Point ((int)(Lcd.Width / 2 - titleSize / 2), (int)(startPoint1.Y - (font.maxHeight / 2))), new Point ((int)(Lcd.Width / 2 + titleSize / 2), (int)(startPoint1.Y + (font.maxHeight / 2))));
			token = cancelTokenSource.Token;
			
						
			int top = dialogWindowInner.P1.Y + (int)( f.maxHeight/2) + topOffset;
			int middel = dialogWindowInner.P1.Y  + ((dialogWindowInner.P2.Y - dialogWindowInner.P1.Y) / 2) - (int)(f.maxHeight)/2;
			int count = 0;
			while (middel > top) {
				middel = middel-(int)f.maxHeight;
				count ++;
			}
			numberOfLines = count*2+1;
			Point start1 = new Point (dialogWindowInner.P1.X, topOffset+  dialogWindowInner.P1.Y  + ((dialogWindowInner.P2.Y - dialogWindowInner.P1.Y) / 2) - (int)f.maxHeight/2 - count*((int)f.maxHeight) );
			Point start2 = new Point (dialogWindowInner.P2.X, start1.Y + (int)f.maxHeight);
			lines = new List<Rectangle>();
			for(int i = 0; i < numberOfLines; i++){
				lines.Add(new Rectangle(new Point(start1.X, start1.Y+(i*(int)f.maxHeight)),new Point(start2.X,start2.Y+(i*(int)f.maxHeight))));	
            }
			bottomLineCenter = new Point(dialogWindowInner.P1.X + ((dialogWindowInner.P2.X-dialogWindowInner.P1.X)/2) , dialogWindowOuther.P2.Y - dialogEdge/2);
			
		}
		
		protected void Cancel()
		{
			cancelTokenSource.Cancel();	
		}
		
		public virtual bool Show()
		{
			bool exit = false;
			OnShow();
			while (!exit && !token.IsCancellationRequested) {
				Draw ();
				switch (btns.GetKeypress(token)) {
					case Buttons.ButtonStates.Down: 
						if (OnDownAction()) 
						{
							exit = true;
						}
						break;
					case Buttons.ButtonStates.Up:
						if (OnUpAction ()) 
						{
							exit = true;
						}
						break;
					case Buttons.ButtonStates.Escape:
						if (OnEscape()) 
						{
							exit = true;
						}
						break;
					case Buttons.ButtonStates.Enter:
						if (OnEnterAction ()) 
						{
							exit = true;
						}
						break;
					case Buttons.ButtonStates.Left:
						if (OnLeftAction()) 
						{
							exit = true;
						}
						break;
					case Buttons.ButtonStates.Right:
						if (OnRightAction()) 
						{
							exit = true;
						}
						break;
				}
			}
			OnExit();
			return true;
		}
		
		protected virtual bool OnEnterAction ()
		{
			return false;
		}
		
		protected virtual bool OnLeftAction ()
		{
			return false;
		}
		
		protected virtual bool OnRightAction ()
		{
			return false;
		}
		
		protected virtual bool OnUpAction ()
		{
			return false;
		}
		
		protected virtual bool OnDownAction ()
		{
			return false;
		}
		
		protected virtual bool OnEscape(){
			return false;
		}
		
		protected Rectangle GetLineRectangle (int lineIndex)
		{
			return lines[lineIndex];
		}
		
		protected void WriteTextOnLine (string text, int lineIndex, bool color = true, Lcd.Alignment alignment = Lcd.Alignment.Center)
		{
			lcd.WriteTextBox(font, lines[lineIndex], text, color, alignment); 
		}
		
		protected void DrawCenterButton (string text, bool color)
		{
			DrawCenterButton(text,color,0);
		}
		
		protected void DrawCenterButton (string text, bool color, int textSize)
		{
			if (textSize == 0) 
			{
				textSize = font.TextSize(text).X;	
			}
			textSize+= buttonTextOffset;
			Point buttonP1 = bottomLineCenter + new Point((int)-textSize/2,(int)-font.maxHeight/2);
			Point buttonP2 = bottomLineCenter + new Point((int)textSize/2,(int)font.maxHeight/2);
			
			Point buttonP1Outer = buttonP1 + new Point(-buttonEdge,-buttonEdge);
			Point buttonp2Outer = buttonP2 + new Point(buttonEdge,buttonEdge);
			
			Rectangle buttonRect = new Rectangle(buttonP1, buttonP2);
			Rectangle buttonRectEdge = new Rectangle(buttonP1Outer, buttonp2Outer);
			
			lcd.DrawBox(buttonRectEdge,true);
			lcd.WriteTextBox(font,buttonRect,text, color, Lcd.Alignment.Center);		
		}
		
		protected void DrawLeftButton (string text, bool color)
		{
			DrawLeftButton (text, color, 0);
		}
		
		protected void DrawLeftButton (string text, bool color, int textSize)
		{
			
			if (textSize == 0) 
			{
				textSize = font.TextSize(text).X;	
			}
			textSize+= buttonTextOffset;
			Point left1 = bottomLineCenter + new Point(-boxMiddleOffset - (int)textSize,(int)-font.maxHeight/2);
			Point left2 = bottomLineCenter + new Point(-boxMiddleOffset,(int)font.maxHeight/2);
			Point leftOuter1 = left1 + new Point(-buttonEdge,-buttonEdge);
			Point leftOuter2 = left2 + new Point(buttonEdge,buttonEdge);
			
			Rectangle leftRect = new Rectangle(left1, left2);
			Rectangle leftOuterRect = new Rectangle(leftOuter1, leftOuter2);
			
			lcd.DrawBox(leftOuterRect,true);
			lcd.WriteTextBox(font, leftRect, text, color, Lcd.Alignment.Center);
		
		}
		
		protected void DrawRightButton (string text, bool color)
		{
			DrawRightButton (text, color, 0);
		}
		
		protected void DrawRightButton (string text, bool color, int textSize)
		{
			if (textSize == 0) 
			{
				textSize = font.TextSize(text).X;	
			}
			textSize+= buttonTextOffset;
			Point right1 = bottomLineCenter + new Point(boxMiddleOffset,(int)-font.maxHeight/2);
			Point right2 = bottomLineCenter + new Point(boxMiddleOffset + (int)textSize,(int)font.maxHeight/2);
			Point rightOuter1 = right1 + new Point(-buttonEdge,-buttonEdge);
			Point rightOuter2 = right2 + new Point(buttonEdge,buttonEdge);
			
			
			Rectangle rightRect = new Rectangle(right1, right2);
			Rectangle rightOuterRect = new Rectangle(rightOuter1, rightOuter2);
			
			lcd.DrawBox(rightOuterRect, true);
			
			lcd.WriteTextBox(font, rightRect, text, color, Lcd.Alignment.Center);
		
		}
		
		protected void WriteTextOnDialog (string text)
		{
			int width = lines [0].P2.X - lines [0].P1.X;
			int textRectRatio = font.TextSize (text).X / (width);
			if (textRectRatio == 0) {
				int middle = (lines.Count / 2);
				lcd.WriteTextBox (font, lines [middle], text, true, Lcd.Alignment.Center);
			} else {
				string[] words = text.Split (' ');
				int rectIndex = 0;
				string s = "";
				for (int i = 0; i < words.Length; i++) {
					if (font.TextSize (s + " " + words [i]).X < width) {
						if (s == "") {
							s = words [i]; 
						} else {
							s = s + " " + words [i];
						}
					} else {
						lcd.WriteTextBox (font, lines [rectIndex], s, true, Lcd.Alignment.Center);
						s = words [i];
						rectIndex++;
						if (rectIndex >= lines.Count)
							break;
					}  			
				
				}
				if (s != "" && rectIndex < lines.Count) {
					lcd.WriteTextBox (font, lines [rectIndex], s, true, Lcd.Alignment.Center);
				}
			}
		}
		
		protected  abstract void OnDrawContent ();
		
		protected void ClearContent ()
		{
			lcd.LoadScreen();
			lcd.DrawBox(dialogWindowOuther, true);
			lcd.DrawBox(dialogWindowInner, false);
			lcd.WriteTextBox(font,titleRect,title, false,Lcd.Alignment.Center); 
		}
		
		protected virtual void Draw ()
		{
			lcd.DrawBox(dialogWindowOuther, true);
			lcd.DrawBox(dialogWindowInner, false);
			OnDrawContent();
			lcd.WriteTextBox(font,titleRect,title, false,Lcd.Alignment.Center); 
			lcd.Update();
			 
		}
	}
}

