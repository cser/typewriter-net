using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MulticaretEditor.KeyMapping
{
	public class KeyAction
	{
		public readonly string name;
		public readonly Getter<Controller, bool> doOnDown;
		public readonly Setter<Controller, bool> doOnModeChange;
		public readonly bool needScroll;
		
		public KeyAction(string name, Getter<Controller, bool> doOnDown, Setter<Controller, bool> doOnModeChange, bool needScroll)
		{
			this.name = name;
			this.doOnDown = doOnDown;
			this.doOnModeChange = doOnModeChange;
			this.needScroll = needScroll;
		}
		
		private static RWList<KeyAction> actions = new RWList<KeyAction>();
		public static IRList<KeyAction> Actions { get { return actions; } }
		
		private static KeyAction Add(string type, Getter<Controller, bool> doOnDown, Setter<Controller, bool> doOnModeChange, bool needScroll)
		{
			KeyAction action = new KeyAction(type, doOnDown, doOnModeChange, needScroll);
			actions.Add(action);
			return action;
		}
		
		public static readonly KeyAction Home = Add("&Edit\\Selection\\Home", DoHome, null, true);
		private static bool DoHome(Controller controller)
		{
			controller.MoveHome(false);
			return true;
		}
		
		public static readonly KeyAction HomeWithSelection = Add("&Edit\\Selection\\Home with selection", DoHomeWithSelection, null, true);
		private static bool DoHomeWithSelection(Controller controller)
		{
			controller.MoveHome(true);
			return true;
		}
		
		public static readonly KeyAction End = Add("&Edit\\Selection\\End", DoEnd, null, true);
		private static bool DoEnd(Controller controller)
		{
			controller.MoveEnd(false);
			return true;
		}
		
		public static readonly KeyAction EndWithSelection = Add("&Edit\\Selection\\End with selection", DoEndWithSelection, null, true);
		private static bool DoEndWithSelection(Controller controller)
		{
			controller.MoveEnd(true);
			return true;
		}
		
		public static readonly KeyAction Delete = Add("&Edit\\Selection\\Delete", DoDelete, null, true);
		private static bool DoDelete(Controller controller)
		{
			if (controller.AllSelectionsEmpty)
			{
				controller.Delete();
			}
			else
			{
				controller.EraseSelection();
			}
			return true;
		}
		
		public static readonly KeyAction Left = Add("&Edit\\Selection\\Left", DoLeft, null, true);
		private static bool DoLeft(Controller controller)
		{
			controller.MoveLeft(false);
			return true;
		}

		public static readonly KeyAction LeftWithSelection = Add("&Edit\\Selection\\Left with selection", DoLeftWithSelection, null, true);
		private static bool DoLeftWithSelection(Controller controller)
		{
			controller.MoveLeft(true);
			return true;
		}

		public static readonly KeyAction LeftWord = Add("&Edit\\Selection\\Left word", DoLeftWord, null, true);
		private static bool DoLeftWord(Controller controller)
		{
			controller.MoveWordLeft(false);
			return true;
		}
  
		public static readonly KeyAction LeftWordWithSelection = Add("&Edit\\Selection\\Left word with selection", DoLeftWordWithSelection, null, true);
		private static bool DoLeftWordWithSelection(Controller controller)
		{
			controller.MoveWordLeft(true);
			return true;
		}
		
		public static readonly KeyAction Right = Add("&Edit\\Selection\\Right", DoRight, null, true);
		private static bool DoRight(Controller controller)
		{
			controller.MoveRight(false);
			return true;
		}

		public static readonly KeyAction RightWithSelection = Add("&Edit\\Selection\\Right with selection", DoRightWithSelection, null, true);
		private static bool DoRightWithSelection(Controller controller)
		{ 
			controller.MoveRight(true);
			return true;
		}

		public static readonly KeyAction RightWord = Add("&Edit\\Selection\\Right word", DoRightWord, null, true);
		private static bool DoRightWord(Controller controller)
		{ 
			controller.MoveWordRight(false);
			return true;
		}

		public static readonly KeyAction RightWordWithSelection = Add("&Edit\\Selection\\Right word with selection", DoRightWordWithSelection, null, true);
		private static bool DoRightWordWithSelection(Controller controller)
		{
			controller.MoveWordRight(true);
			return true;
		}
		
		public static readonly KeyAction Up = Add("&Edit\\Selection\\Up", DoKeyAction, null, true);
		private static bool DoKeyAction(Controller controller)
		{
			controller.MoveUp(false);
			return true;
		}
		
		public static readonly KeyAction UpWithSelection = Add("&Edit\\Selection\\Up with selection", DoUpWithSelection, null, true);
		private static bool DoUpWithSelection(Controller controller)
		{
			controller.MoveUp(true);
			return true;
		}
		
		public static readonly KeyAction Down = Add("&Edit\\Selection\\Down", DoDown, null, true);
		private static bool DoDown(Controller controller)
		{
			controller.MoveDown(false);
			return true;
		}
		
		public static readonly KeyAction DownWithSelection = Add("&Edit\\Selection\\Down with selection", DoDownWithSelection, null, true);
		private static bool DoDownWithSelection(Controller controller)
		{
			controller.MoveDown(true);
			return true;
		}
		
		public static readonly KeyAction PutCursorUp = Add("&Edit\\Selection\\Put cursor up", DoPutCursorUp, null, true);
		private static bool DoPutCursorUp(Controller controller)
		{
			controller.PutCursorUp();
			return true;
		}
		
		public static readonly KeyAction PutCursorDown = Add("&Edit\\Selection\\Put cursor down", DoPutCursorDown, null, true);
		private static bool DoPutCursorDown(Controller controller)
		{
			controller.PutCursorDown();
			return true;
		}
		
		public static readonly KeyAction SelectAll = Add("&Edit\\Selection\\Select all", DoSelectAll, null, false);
		private static bool DoSelectAll(Controller controller)
		{
			controller.SelectAll();
			return true;
		}
		
		public static readonly KeyAction Copy = Add("&Edit\\Selection\\Copy", DoCopy, null, true);
		private static bool DoCopy(Controller controller)
		{
			controller.Copy();
			return true;
		}
		
		public static readonly KeyAction Paste = Add("&Edit\\Selection\\Paste", DoPaste, null, true);
		private static bool DoPaste(Controller controller)
		{
			controller.Paste();
			return true;
		}
		
		public static readonly KeyAction Cut = Add("&Edit\\Selection\\Cut", DoCut, null, true);
		private static bool DoCut(Controller controller)
		{
			controller.Cut();
			return true;
		}
		
		public static readonly KeyAction ClearMinorSelections = Add("&Edit\\Selection\\Clear minor selections", DoClearMinorSelections, null, true);
		private static bool DoClearMinorSelections(Controller controller)
		{
			return controller.ClearMinorSelections();
		}

		public static readonly KeyAction ClearFirstMinorSelections = Add("&Edit\\Selection\\Clear first minor selections", DoClearFirstMinorSelections, null, true);
		private static bool DoClearFirstMinorSelections(Controller controller)
		{
			return controller.ClearFirstMinorSelections();
		}
		
		public static readonly KeyAction SelectNextText = Add("&Edit\\Selection\\Select next text", DoSelectNextText, null, true);
		private static bool DoSelectNextText(Controller controller)
		{
			controller.SelectNextText();
			return true;
		}
		
		public static readonly KeyAction ShiftLeft = Add("&Edit\\Text\\Shift left", DoShiftLeft, null, true);
		private static bool DoShiftLeft(Controller controller)
		{
			return controller.ShiftLeft();
		}
		
		public static readonly KeyAction ShiftRight = Add("&Edit\\Text\\Shift right", DoShiftRight, null, true);
		private static bool DoShiftRight(Controller controller)
		{
			return controller.ShiftRight();
		}
		
		public static readonly KeyAction RemoveWordLeft = Add("&Edit\\Text\\Remove word left", DoRemoveWordLeft, null, true);
		private static bool DoRemoveWordLeft(Controller controller)
		{
			return controller.RemoveWordLeft();
		}
		
		public static readonly KeyAction RemoveWordRight = Add("&Edit\\Text\\Remove word right", DoRemoveWordRight, null, true);
		private static bool DoRemoveWordRight(Controller controller)
		{
			return controller.RemoveWordRight();
		}
		
		public static readonly KeyAction MoveLineUp = Add("&Edit\\Text\\Move line up", DoMoveLineUp, null, true);
		private static bool DoMoveLineUp(Controller controller)
		{
			return controller.MoveLineUp();
		}
		
		public static readonly KeyAction MoveLineDown = Add("&Edit\\Text\\Move line down", DoMoveLineDown, null, true);
		private static bool DoMoveLineDown(Controller controller)
		{
			return controller.MoveLineDown();
		}
		
		public static readonly KeyAction Undo = Add("&Edit\\Undo", DoUndo, null, true);
		private static bool DoUndo(Controller controller)
		{
			controller.Undo();
			return true;
		}
		
		public static readonly KeyAction Redo = Add("&Edit\\Redo", DoRedo, null, true);
		private static bool DoRedo(Controller controller)
		{
			controller.Redo();
			return true;
		}
		
		public static readonly KeyAction SwitchBranch = Add("&Edit\\Switch branch", DoSwitchBranch, DoSwitchBranchCangeMode, true);
		private static bool DoSwitchBranch(Controller controller)
		{
			controller.history.TagsDown();
			return true;
		}
		private static void DoSwitchBranchCangeMode(Controller controller, bool mode)
		{
			if (mode)
			{
				controller.history.TagsModeOn();
			}
			else
			{
				controller.history.TagsModeOff();
			}
		}
		
		public static readonly KeyAction PageUp = Add("&Edit\\Selection\\Page up", DoPageUp, null, false);
		private static bool DoPageUp(Controller controller)
		{
			controller.ScrollPage(true, false);
			return true;
		}
		
		public static readonly KeyAction PageDown = Add("&Edit\\Selection\\Page down", DoPageDown, null, false);
		private static bool DoPageDown(Controller controller)
		{
			controller.ScrollPage(false, false);
			return true;
		}
		
		public static readonly KeyAction PageUpWithSelection = Add("&Edit\\Selection\\Page up with selection", DoPageUpWithSelection, null, false);
		private static bool DoPageUpWithSelection(Controller controller)
		{
			controller.ScrollPage(true, true);
			return true;
		}
		
		public static readonly KeyAction PageDownWithSelection = Add("&Edit\\Selection\\Page down with selection", DoPageDownWithSelection, null, false);
		private static bool DoPageDownWithSelection(Controller controller)
		{
			controller.ScrollPage(false, true);
			return true;
		}
		
		public static readonly KeyAction ScrollUp = Add("&Edit\\Selection\\Scroll up", DoScrollUp, null, false);
		private static bool DoScrollUp(Controller controller)
		{
			controller.Scroll(0, -1);
			return true;
		}
		
		public static readonly KeyAction ScrollDown = Add("&Edit\\Selection\\Scroll down", DoScrollDown, null, false);
		private static bool DoScrollDown(Controller controller)
		{
			controller.Scroll(0, 1);
			return true;
		}

		public static readonly KeyAction DocumentStart = Add("&Edit\\Selection\\Document start", DoDocumentStart, null, true);
		private static bool DoDocumentStart(Controller controller)
		{
			controller.DocumentStart(false);
			return true;
		}

		public static readonly KeyAction DocumentEnd = Add("&Edit\\Selection\\Document end", DoDocumentEnd, null, true);
		private static bool DoDocumentEnd(Controller controller)
		{
			controller.DocumentEnd(false);
			return true;
		}

		public static readonly KeyAction DocumentStartWithSelection = Add("&Edit\\Selection\\Document start with selection", DoDocumentStartWithSelection, null, true);
		private static bool DoDocumentStartWithSelection(Controller controller)
		{
			controller.DocumentStart(true);
			return true;
		}

		public static readonly KeyAction DocumentEndWithSelection = Add("&Edit\\Selection\\Document end with selection", DoDocumentEndWithSelection, null, true);
		private static bool DoDocumentEndWithSelection(Controller controller)
		{
			controller.DocumentEnd(true);
			return true;
		}
		
		public static readonly KeyAction Nothing = new KeyAction("Nothing", DoNothing, null, false);
		private static bool DoNothing(Controller controller)
		{
			return true;
		}
	}
}
