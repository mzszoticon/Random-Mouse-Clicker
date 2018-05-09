using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Random_Mouse_Clicker.Generators;

namespace Random_Mouse_Clicker
{
    public partial class MainForm : Form
    {
        private bool widthNotZero;
        private bool heightNotZero;
        private int displayedWidth;
        private int displayedHeight;
        private readonly int originalFormWidth;
        private readonly int originalFormHeight;
        private readonly Random random = new Random();
        private Action<Point> moveAtMouseSpeed;
        private decimal[] minMax = new decimal[2];
        private readonly Hotkey hotkey = new Hotkey();
        private INextPointGenerator generator = null;

        /**
         * Initialize the MainForm and store width and height information
         * Set indexes of comboboxes so that they aren't blank
         * Add event listener for when tab is changed
         * */
        public MainForm()
        {
            InitializeComponent();

            originalFormWidth = this.Width;
            originalFormHeight = this.Height;
            comboBoxClickEvery.SelectedIndex = 1;
            comboBoxDuration.SelectedIndex = 0;

            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            forceAutomaticEnd();
        }

        /**
         * Once form is loaded, register the hot key
         * */
        private void Form1_Load(object sender, EventArgs e)
        {
            RegisterHotKey();
        }

        /**
         * Start snipping when select area is clicked
         * Make the start button clickable
         * */
        private void selectAreaButton_Click(object sender, EventArgs e)
        {
            SnippingTool.Snip();
            buttonStart.Enabled = true;
        }

        /**
         * When start button clicked, stores rectangle coordinates
         * x1 and x2 are the upper left and right corner
         * x1 and x2 are the lower left and right corner
         * checkClickInterval runs to see the time between clicks
         * Checks mouse speed function needed and stores the method to a variable
         * runManualOrAutomatic runs to see how the program will end
         * */
        private void startButton_Click(object sender, EventArgs e)
        {
            Rectangle bounds = SnippingTool.getDrawnRectangle();
            bounds.Offset(SnippingTool.clickingScreen.Bounds.Location);
            checkClickInterval(comboBoxClickEvery, numericClickEveryMin.Value, numericClickEveryMax.Value);
            moveAtMouseSpeed = checkMouseSpeed();

            bool isSplit = checkBoxDivideInto.Checked && comboBoxDividedAreas.SelectedIndex != -1;
            decimal clicks = checkClickDuration(comboBoxDuration, numericDuration.Value);
            if (isSplit)
            {
                clicks = numericClickEachArea.Value;
            }

            checkClickInterval(comboBoxClickEvery, numericClickEveryMin.Value, numericClickEveryMax.Value);
            moveAtMouseSpeed = checkMouseSpeed();

            runManualOrAutomatic(bounds, isSplit ? ImageSplitter.dimensions[comboBoxDividedAreas.SelectedIndex].Item1 : 1, isSplit ? ImageSplitter.dimensions[comboBoxDividedAreas.SelectedIndex].Item2 : 1, false, (int)clicks);
        }

        /**
         * Checks combo box text field to set the time
         * Stores minimum and maximum value in an array called minMax
         */
        private void checkClickInterval(ComboBox comboBox, decimal min, decimal max)
        {
            if (comboBox.Text == "millisecond(s)")
            {
                minMax[0] = min;
                minMax[1] = max;
            }

            else if (comboBox.Text == "second(s)")
            {
                minMax[0] = min * 1000;
                minMax[1] = max * 1000;
            }

            else if (comboBox.Text == "minute(s)")
            {
                minMax[0] = min * 1000 * 60;
                minMax[1] = max * 1000 * 60;
            }

            else if (comboBox.Text == "hour(s)")
            {
                minMax[0] = min * 1000 * 60 * 60;
                minMax[1] = max * 1000 * 60 * 60;
            }

            else if (comboBox.Text == "day(s)")
            {
                minMax[0] = min * 1000 * 60 * 60 * 24;
                minMax[1] = max * 1000 * 60 * 60 * 24;
            }
        }

        /**
         * Checks what the mouse speed is set to
         * Returns the function that should be ran
         * */
        private Action<Point> checkMouseSpeed()
        {
            if (radioSlow.Checked)
            {
                return MouseLinearSmoothMove.slow;
            }

            else if (radioNormal.Checked)
            {
                return MouseLinearSmoothMove.normal;
            }

            else if (radioFast.Checked)
            {
                return MouseLinearSmoothMove.fast;
            }

            else
            {
                return MouseLinearSmoothMove.instant;
            }
        }

        /**
         * For manual, program will only stop when user presses CTRL+WIN+ESC
         * Minimizes form so window is not in the way
         * Performs clicking operations
         *
         * For automatic, checks the duration from the combo box
         * Starts a stopwatch to keep track of time
         * If the duration is not zero, will begin clicking
         * The checkClickDuration method will return 0 if being automatically ended by a certain number of clicks, rather than a numeric amount of time
         * */
        private void runManualOrAutomatic(Rectangle bounds, int rows, int cols, bool sequential, int repeats)
        {
            if (rows == 1 && cols == 1)
            {
                generator = new RandomPointGenerator(random, bounds, repeats);
            }
            else
            {
                if (sequential)
                {
                    generator = new SequentialSplitAreaPointGenerator(random, bounds, rows, cols, repeats);
                }
                else
                {
                    generator = new RandomSplitAreaPointGenerator(random, bounds, rows, cols, repeats);
                }
            }

            decimal duration = checkClickDuration(comboBoxDuration, numericDuration.Value);
            if (duration > 0)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                ShowBalloonMessage("Program will end after " + numericDuration.Value + " " + comboBoxDuration.Text + " or when CTRL+WIN+ESC is pressed" +
                                   "...", "Random Mouse Clicker");
                this.WindowState = FormWindowState.Minimized;

                clickUntilAutomaticallyEnded(duration, stopwatch);
            }
            else
            {

                ShowBalloonMessage("Press CTRL+WIN+ESC to exit the program...", "Random Mouse Clicker");
                this.WindowState = FormWindowState.Minimized;
                clickUntilManuallyEnded();
            }
        }

        /**
         * Returns the duration of time to click for, if ending automatically
         * If the combo box is set to an amount of clicks, will return 0 since clicks aren't time based
         * Checks if user selected to divide portions of the screen into areas
         * Otherwise, will calculate and return the duration based on the time setting
         * */
        private decimal checkClickDuration(ComboBox comboBox, decimal duration)
        {
            if (comboBox.Text == "click(s)")
            {
                return 0;
            }

            else if (comboBox.Text == "millisecond(s)")
            {
                return duration;
            }

            else if (comboBox.Text == "second(s)")
            {
                return duration * 1000;
            }

            else if (comboBox.Text == "minute(s)")
            {
                return duration * 1000 * 60;
            }

            else if (comboBox.Text == "hour(s)")
            {
                return duration * 1000 * 60 * 60;
            }

            else if (comboBox.Text == "day(s)")
            {
                return duration * 1000 * 60 * 60 * 24;
            }

            return 0;
        }

        private decimal getClicks(ComboBox comboBox, decimal clicks)
        {
            if (comboBox.Text == "click(s)")
            {
                return clicks;
            }

            return -1;
        }

        /**
         * Starts a new thread so the hotkey has no issues exiting the program
         * Runs an infinite loop and performs clicking in random locations
         * */
        private void clickUntilManuallyEnded()
        {

            new Thread(delegate () {

                while (generator.HasNextPoint)
                {
                    randomizeLocationAndClick();
                }

            }).Start();
        }

        /**
         * Starts a new thread so the hotkey has no issues exiting the program
         * Runs until the elapsed amount of time exceeds the defined duration
         * */
        private void clickUntilAutomaticallyEnded(decimal duration, Stopwatch stopwatch)
        {
            new Thread(delegate () {

                while (generator.HasNextPoint)
                {
                    if (stopwatch.ElapsedMilliseconds > duration)
                    {
                        ShowBalloonMessage("Program has finished clicking", "Random Mouse Clicker");
                        break;
                    }
                    else
                    {
                        randomizeLocationAndClick();
                    }
                }

            }).Start();
        }

        /**
         * Randomizes the x,y coordinates and sets the location to within the user's selection
         * Adds monitor offset if clicking at a secondary monitor
         * Moves mouse at specified speed
         * Clicks once at the location
         * */
        private void randomizeLocationAndClick()
        {
            var nextPoint = generator.GetNextPoint();
            moveAtMouseSpeed(nextPoint);
            clickAndWait();
        }

        /**
         * Performs a mouse click
         * Waits for a random amount of time, defined by the user's minimum and maximum
         * */
        private void clickAndWait()
        {
            MouseActions.MouseClick();
            Thread.Sleep(random.Next((int)minMax[0], (int)minMax[1]));
        }

        /**
         * When choosing to end manually, disable the automatic duration form components
         * */

        private void endManuallyRadio_CheckedChanged(object sender, EventArgs e)
        {
            setAutomaticDuration(false);
        }

        /**
         * When choosing to end automatically, enable the automatic duration form components
         * */
        private void endAutomaticallyRadio_CheckedChanged(object sender, EventArgs e)
        {
            setAutomaticDuration(true);
        }

        /**
         * Set automatic duration form components on or off
         * */
        private void setAutomaticDuration(bool b)
        {
            groupBoxDuration.Enabled = b;
            numericDuration.Enabled = b;
            comboBoxDuration.Enabled = b;
        }

        /**
         * Listener method that runs when a tab is clicked
         * If basic tab selected, resize form back to original size
         * If advanced tab selected, set the label and other components accordingly
         * If the displayed width or height does not match the rectangle, update the display
         * If preview tab selected, show the user's selection
         * Resizes the preview tab to display entire image, accounting for the form's borders cutting off part of the image
         * If splitting the region into areas, the preview tab will show the image with red lines drawn as dividers
         * */
        private void tabControl1_SelectedIndexChanged(Object sender, EventArgs e)
        {
            bool basicTabSelected = tabControl1.SelectedIndex == 0;
            bool advancedTabSelected = tabControl1.SelectedIndex == 1;
            bool previewTabSelected = tabControl1.SelectedIndex == 2;
            widthNotZero = SnippingTool.getRectangleWidth() != 0;
            heightNotZero = SnippingTool.getRectangleHeight() != 0;

            if (basicTabSelected)
            {
                resizeFormToDefault();
            }
            else if (advancedTabSelected && widthNotZero && heightNotZero)
            {
                resizeFormToDefault();

                labelWidthHeight.Text = "The area has a width of " + SnippingTool.getRectangleWidth() + " pixels\r\n"
                + " and a height of " + SnippingTool.getRectangleHeight() + " pixels";

                checkBoxDivideInto.Enabled = true;

                if (displayedWidth != SnippingTool.getRectangleWidth() || displayedHeight != SnippingTool.getRectangleHeight())
                {
                    divideIntoEqualAreasDisplay();
                }
            }
            else if (previewTabSelected && widthNotZero && heightNotZero)
            {
                previewPictureBox.Visible = true;
                labelPreviewInstructions.Visible = false;

                if (SnippingTool.Image.Width > tabControl1.Width)
                {
                    this.Width = SnippingTool.Image.Width + (this.Width - tabControl1.Width) + 8;
                }

                if (SnippingTool.Image.Height > tabControl1.Height)
                {
                    this.Height = SnippingTool.Image.Height + (this.Height - tabControl1.Height) + 25;
                }

                if (checkBoxDivideInto.Checked)
                {
                   ImageSplitter.drawSplitImage(comboBoxDividedAreas.SelectedIndex, numericDivideIntoEqualAreas.Value);
                   previewPictureBox.Image = ImageSplitter.drawnImage;
                }
                else
                {
                    previewPictureBox.Image = SnippingTool.Image;
                }
            }
        }

        /**
         * Make form go back to the original size
         * Used when going from the preview tab back to the basic or advanced tab
         * */
        private void resizeFormToDefault()
        {
            this.Width = originalFormWidth;
            this.Height = originalFormHeight;
        }

        /**
         * If choosing to divide region into areas, then enable the advanced form components
         * Forces the use of an automatic end because the user can set the amount of clicks per area
         * Otherwise when unchecked, sets back to manual end and disables advanced form components
         * */
        private void divideIntoEqualAreasCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDivideInto.Checked && widthNotZero && heightNotZero)
            {
                divideIntoEqualAreasDisplay();
                enableDividedAreas(true);
                //forceAutomaticEnd();
            }
            else
            {
                enableDividedAreas(false);
                //setBackToManualEnd();
            }
        }

        /**
         * Enable the components that divide the region
         * */
        private void enableDividedAreas(bool b)
        {
            numericDivideIntoEqualAreas.Enabled = b;
            labelClickEachArea.Enabled = b;
            labelClickEachTimes.Enabled = b;
            labelOf.Enabled = b;
            labelCannotBeChanged.Visible = b;
            comboBoxDividedAreas.Enabled = b;
            numericClickEachArea.Enabled = b;
        }

        /**
         * Sets form components to match ending automatically through a number of clicks
         * Used when dividing region into areas
         * */
        private void forceAutomaticEnd()
        {
            radioEndAutomatically.Checked = true;
            radioEndManually.Enabled = false;
            comboBoxDuration.Enabled = false;
            comboBoxDuration.SelectedIndex = 0;
            numericDuration.Value = updateTotalClicksDisplay();
            numericDuration.Enabled = false;
        }

        /**
         * Enables form compnents to accomodate ending manually
         * */
        private void setBackToManualEnd()
        {
            radioEndManually.Checked = true;
            radioEndManually.Enabled = true;
            numericDuration.Value = 1;
        }

        /**
         * Updates based on the number of areas the user wants to divide into, minimum and default is 2
         * */
        private void numericDivideIntoEqualAreas_ValueChanged(object sender, EventArgs e)
        {
            divideIntoEqualAreasDisplay();
            numericDuration.Value = updateTotalClicksDisplay();
        }

        /**
         * Update the combo box to provide all the different ways the area can be divided into
         * Gets dimensions based on number of areas
         * Clears combo box of any previous data
         * Adds dimension selection into the combo box
         * Stores the displayed width and height for error checking
         * Displayed width is always at the beginning of the width array, and height at the end of the height array
         * */
        private void divideIntoEqualAreasDisplay()
        {
            ImageSplitter.getDimensions((int)numericDivideIntoEqualAreas.Value);
            comboBoxDividedAreas.Items.Clear();

            for (int i = 0; i < ImageSplitter.dimensions.Count(); i++)
            {
                String s = ImageSplitter.dimensionWidths[i] + " x " + ImageSplitter.dimensionHeights[i];
                comboBoxDividedAreas.Items.Add(s);
            }
            displayedWidth = ImageSplitter.dimensionWidths[0];
            displayedHeight = ImageSplitter.dimensionHeights[ImageSplitter.dimensionHeights.Count - 1];
        }

        /**
        * When changing the amount of times each area will be clicked, updates the total of clicks
        * */
        private void numericClickEachArea_ValueChanged(object sender, EventArgs e)
        {
            numericDuration.Value = updateTotalClicksDisplay();
        }

        /**
         * Update the total amount of clicks needed for the automatic end
         * */
        private int updateTotalClicksDisplay()
        {
            return (int)numericDivideIntoEqualAreas.Value * (int)numericClickEachArea.Value;
        }

       /**
        * Shows tooltip balloon message on the taskbar
        * */
        private void ShowBalloonMessage(string text, string title)
        {
            notifyIcon.BalloonTipText = text;
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.ShowBalloonTip(1000);
        }

        /**
         * Registers the hotkey
         * */
        private void RegisterHotKey()
        {
            hotkey.Control = true;
            hotkey.Windows = true;
            hotkey.KeyCode = Keys.Escape;

            hotkey.Pressed += Hk_Win_ESC_OnPressed;

            if (!hotkey.GetCanRegister(this))
            {
                Console.WriteLine("Already registered");
            }
            else
            {
                hotkey.Register(this);
            }
        }

        /**
         * When hotkey is pressed, exits program
         * */
        private void Hk_Win_ESC_OnPressed(object sender, HandledEventArgs handledEventArgs)
        {
            Exit();
        }

        /**
         * Unregisters hotkey
         * */
        private void UnregisterHotkey()
        {
            if (hotkey.Registered)
            {
                hotkey.Unregister();
            }
        }

        /**
        * Exits program
        * */
        private void menuExit_Click_1(object sender, EventArgs e)
        {
            Exit();
        }

        /**
         * Hides and removes taskbar icon
         * Unregisters hotkey from windows
         * Exits application
         * */
        private void Exit()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            UnregisterHotkey();
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
