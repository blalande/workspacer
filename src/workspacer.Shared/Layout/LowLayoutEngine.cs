using System;
using System.Collections.Generic;
using System.Linq;

namespace workspacer
{
    public class LowLayoutEngine : ILayoutEngine
    {
        private readonly int _numInPrimary;
        private readonly double _primaryPercent;
        private readonly double _primaryPercentIncrement;
        private readonly bool _leftToRight;

        private int _numInPrimaryOffset = 0;
        private double _primaryPercentOffset = 0;
        private double _mainWindowPercent;

        public LowLayoutEngine(bool reversed = false) : this(1, 0.25, 0.03, reversed) { }

        public LowLayoutEngine(int numInPrimary, double primaryPercent, double primaryPercentIncrement, bool reversed)
        {
            _numInPrimary = numInPrimary;
            _primaryPercent = primaryPercent;
            _primaryPercentIncrement = primaryPercentIncrement;
            _leftToRight = !reversed;
            _mainWindowPercent = 0.8;
        }

        public string Name => "tall";

        public IEnumerable<IWindowLocation> CalcLayout(IEnumerable<IWindow> windows, int spaceWidth, int spaceHeight)
        {
            var list = new List<IWindowLocation>();
            var numWindows = windows.Count();

            if (numWindows == 0)
                return list;

            int numInPrimary = Math.Min(GetNumInPrimary(), numWindows);

            int primaryWidth = spaceWidth / numInPrimary;
            int primaryHeight = (int)(spaceHeight * (_primaryPercent + _primaryPercentOffset));
            int height = spaceHeight / Math.Max(numWindows - numInPrimary, 1);

            // if there are more "primary" windows than actual windows,
            // then we want the pane to actually spread the entire width
            // of the working area
            if (numInPrimary >= numWindows)
            {
                primaryWidth = spaceWidth;
            }

            // main window width will be 80% of total
            int nbSmallWindows = (numWindows - numInPrimary - 1) > 0 ? (numWindows - numInPrimary - 1) : 0;
            int secondaryWidth = (int)((spaceWidth * (1 - _mainWindowPercent)) / (nbSmallWindows == 0 ? 1 : nbSmallWindows));
            int mainWidth = (int)(spaceWidth * _mainWindowPercent);
            int secondaryHeight = spaceHeight - primaryHeight;
            int i = 0;
            int cumulatedSecondaryWidth = 0;
            foreach (var window in windows)
            {

                if (i < numInPrimary)
                {
                    // primary zone at the bottom of the screen
                    list.Add(new WindowLocation(i * primaryWidth, spaceHeight - primaryHeight, primaryWidth, primaryHeight, WindowState.Normal));
                }
                else
                {
                    int windowWidth = secondaryWidth;
                    // secondary zone
                    if (window.IsFocused)
                    {
                        windowWidth = mainWidth;
                    }
                    list.Add(new WindowLocation(cumulatedSecondaryWidth, 0, windowWidth, secondaryHeight, WindowState.Normal));
                    cumulatedSecondaryWidth += windowWidth;
                }
                i++;
            }
            return list;
        }

        public void ShrinkPrimaryArea()
        {
            _primaryPercentOffset -= _primaryPercentIncrement;
        }

        public void ExpandPrimaryArea()
        {
            _primaryPercentOffset += _primaryPercentIncrement;
        }

        public void ResetPrimaryArea()
        {
            _primaryPercentOffset = 0;
        }

        public void IncrementNumInPrimary()
        {
            _numInPrimaryOffset++;
        }

        public void DecrementNumInPrimary()
        {
            if (GetNumInPrimary() > 1)
            {
                _numInPrimaryOffset--;
            }
        }

        private int GetNumInPrimary()
        {
            return _numInPrimary + _numInPrimaryOffset;
        }

        private int CalcXPos(int x, int windowsWidth, int spaceWidth)
        {
            return _leftToRight ? x : spaceWidth - x - windowsWidth;
        }
    }
}
