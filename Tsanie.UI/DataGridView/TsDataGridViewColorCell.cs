using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Tsanie.Utils;

namespace Tsanie.UI {
    public class TsDataGridViewColorCell : DataGridViewTextBoxCell {

        // Default dimensions of the static rendering bitmap used for the painting of the non-edited cells
        private const int TSDATAGRIDVIEWCOLORCELL_defaultRenderingBitmapWidth = 100;
        private const int TSDATAGRIDVIEWCOLORCELL_defaultRenderingBitmapHeight = 22;

        // The bitmap used to paint the non-edited cells
        [ThreadStatic]
        private static Bitmap renderingBitmap;
        // The Label control used to paint the non-edited cells via a call to Label.DrawToBitmap
        [ThreadStatic]
        private static Label paintingLabel;

        public TsDataGridViewColorCell()
            : base() {
            // Create a thread specific bitmap used for the painting of the non-edited cells
            if (renderingBitmap == null) {
                renderingBitmap = new Bitmap(TSDATAGRIDVIEWCOLORCELL_defaultRenderingBitmapWidth,
                                             TSDATAGRIDVIEWCOLORCELL_defaultRenderingBitmapHeight);
            }
            // Create a thread specific Label control used for the painting of the non-edited cells
            if (paintingLabel == null) {
                paintingLabel = new Label();
            }
            paintingLabel.AutoEllipsis = true;
            paintingLabel.AutoSize = false;
        }

        public override void InitializeEditingControl(
            int rowIndex,
            object initialFormattedValue,
            DataGridViewCellStyle dataGridViewCellStyle
        ) {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            TextBox txt = DataGridView.EditingControl as TextBox;
            txt.Text = (string)initialFormattedValue;
        }

        private static readonly Type defaultValueType = typeof(System.Drawing.Color);

        public override Type ValueType {
            get {
                return defaultValueType;
            }
        }

        protected override object GetFormattedValue(
            object value,
            int rowIndex,
            ref DataGridViewCellStyle cellStyle,
            System.ComponentModel.TypeConverter valueTypeConverter,
            System.ComponentModel.TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context
        ) {
            return ((Color)value).ToColorString();
        }

        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts
        ) {
            if (this.DataGridView == null) {
                return;
            }

            // First paint the borders and background of the cell.
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle,
                       paintParts & ~(DataGridViewPaintParts.ErrorIcon |
                                      DataGridViewPaintParts.ContentForeground |
                                      DataGridViewPaintParts.Background));

            Point ptCurrentCell = this.DataGridView.CurrentCellAddress;
            bool cellCurrent = ptCurrentCell.X == this.ColumnIndex && ptCurrentCell.Y == rowIndex;
            bool cellEdited = cellCurrent && this.DataGridView.EditingControl != null;

            // If the cell is in editing mode, there is nothing else to paint
            if (!cellEdited) {
                if (PartPainted(paintParts, DataGridViewPaintParts.ContentForeground)) {
                    // Take the borders into account
                    Rectangle borderWidths = BorderWidths(advancedBorderStyle);
                    Rectangle valBounds = cellBounds;
                    valBounds.Offset(borderWidths.X, borderWidths.Y);
                    valBounds.Width -= borderWidths.Right;
                    valBounds.Height -= borderWidths.Bottom;
                    // Also take the padding into account
                    if (cellStyle.Padding != Padding.Empty) {
                        if (this.DataGridView.RightToLeft == RightToLeft.Yes) {
                            valBounds.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                        } else {
                            valBounds.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
                        }
                        valBounds.Width -= cellStyle.Padding.Horizontal;
                        valBounds.Height -= cellStyle.Padding.Vertical;
                    }
                    // Determine the NumericUpDown control location
                    valBounds = GetAdjustedEditingControlBounds(valBounds, cellStyle);

                    bool cellSelected = (cellState & DataGridViewElementStates.Selected) != 0;

                    if (renderingBitmap.Width < valBounds.Width ||
                        renderingBitmap.Height < valBounds.Height) {
                        // The static bitmap is too small, a bigger one needs to be allocated.
                        renderingBitmap.Dispose();
                        renderingBitmap = new Bitmap(valBounds.Width, valBounds.Height);
                    }

                    // Make sure the NumericUpDown control is parented to a visible control
                    if (paintingLabel.Parent == null || !paintingLabel.Parent.Visible) {
                        paintingLabel.Parent = this.DataGridView;
                    }
                    // Set all the relevant properties
                    paintingLabel.TextAlign = TsDataGridViewColorCell.TranslateAlignment(cellStyle.Alignment);
                    paintingLabel.Font = cellStyle.Font;
                    paintingLabel.Width = valBounds.Width - 14;
                    paintingLabel.Height = valBounds.Height;
                    paintingLabel.RightToLeft = this.DataGridView.RightToLeft;
                    paintingLabel.Location = new Point(0, -paintingLabel.Height - 100);
                    paintingLabel.Text = formattedValue as string;

                    Color valueColor = cellStyle.BackColor;
                    Color backColor = this.DataGridView.DefaultCellStyle.BackColor;
                    if (PartPainted(paintParts, DataGridViewPaintParts.SelectionBackground) && cellSelected) {
                        paintingLabel.ForeColor = cellStyle.SelectionForeColor;
                        backColor = cellStyle.SelectionBackColor;
                    } else {
                        if (rowIndex % 2 == 1) {
                            backColor = this.DataGridView.AlternatingRowsDefaultCellStyle.BackColor;
                        }
                        paintingLabel.ForeColor = cellStyle.ForeColor;
                    }
                    paintingLabel.BackColor = backColor;
                    if (PartPainted(paintParts, DataGridViewPaintParts.Background)) {
                        if (valueColor.A < 255) {
                            // The NumericUpDown control does not support transparent back colors
                            valueColor = Color.FromArgb(255, valueColor);
                        }
                    }
                    // Finally paint the NumericUpDown control
                    Rectangle srcRect = new Rectangle(0, 0, valBounds.Width, valBounds.Height);
                    if (srcRect.Width > 14 && srcRect.Height > 0) {
                        paintingLabel.DrawToBitmap(renderingBitmap, srcRect);
                        graphics.DrawImage(renderingBitmap, new Rectangle(valBounds.Location, valBounds.Size),
                                           srcRect, GraphicsUnit.Pixel);
                    }
                    graphics.FillRectangle(new SolidBrush(valueColor), valBounds.X + valBounds.Width - 14, valBounds.Y,
                        20, valBounds.Height);
                }
                if (PartPainted(paintParts, DataGridViewPaintParts.ErrorIcon)) {
                    // Paint the potential error icon on top of the NumericUpDown control
                    base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText,
                               cellStyle, advancedBorderStyle, DataGridViewPaintParts.ErrorIcon);
                }
            }
        }

        /// <summary>
        /// Little utility function called by the Paint function to see if a particular part needs to be painted. 
        /// </summary>
        private static bool PartPainted(DataGridViewPaintParts paintParts, DataGridViewPaintParts paintPart) {
            return (paintParts & paintPart) != 0;
        }

        /// <summary>
        /// Adjusts the location and size of the editing control given the alignment characteristics of the cell
        /// </summary>
        private Rectangle GetAdjustedEditingControlBounds(Rectangle editingControlBounds, DataGridViewCellStyle cellStyle) {
            // Add a 1 pixel padding on the left and right of the editing control
            editingControlBounds.Width = Math.Max(0, editingControlBounds.Width - 2);

            // Adjust the vertical location of the editing control:
            int preferredHeight = cellStyle.Font.Height + 8;
            if (preferredHeight < editingControlBounds.Height) {
                switch (cellStyle.Alignment) {
                    case DataGridViewContentAlignment.MiddleLeft:
                    case DataGridViewContentAlignment.MiddleCenter:
                    case DataGridViewContentAlignment.MiddleRight:
                        editingControlBounds.Y += (editingControlBounds.Height - preferredHeight) / 2;
                        break;
                    case DataGridViewContentAlignment.BottomLeft:
                    case DataGridViewContentAlignment.BottomCenter:
                    case DataGridViewContentAlignment.BottomRight:
                        editingControlBounds.Y += editingControlBounds.Height - preferredHeight;
                        break;
                }
            }

            return editingControlBounds;
        }

        /// <summary>
        /// Little utility function used by both the cell and column types to translate a DataGridViewContentAlignment value into
        /// a HorizontalAlignment value.
        /// </summary>
        internal static ContentAlignment TranslateAlignment(DataGridViewContentAlignment align) {
            if (align == DataGridViewContentAlignment.NotSet)
                return ContentAlignment.MiddleLeft;
            return (ContentAlignment)align;
        }

    }
}
