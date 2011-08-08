using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Tsanie.Utils;

namespace Tsanie.UI {
    public class TsDataGridViewColorEditingControl : TextBox, IDataGridViewEditingControl {
        private DataGridView grid;
        private bool valueChanged = false;
        private int rowIndex;

        private Color colorValue;

        public TsDataGridViewColorEditingControl() { }

        #region IDataGridViewEditingControl Members

        public object EditingControlFormattedValue {
            get { return this.colorValue.ToColorString(); }
            set {
                string val = value as string;
                if (val != null) {
                    this.colorValue = val.ToColor();
                    this.Text = val;
                }
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
            this.Font = dataGridViewCellStyle.Font;
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            this.BackColor = Color.White;
        }

        public int EditingControlRowIndex {
            get { return rowIndex; }
            set { rowIndex = value; }
        }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) {
            // Let the TextBox handle the keys listed.
            switch (keyData & Keys.KeyCode) {
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.PageDown:
                case Keys.PageUp:
                    return true;
                default:
                    return false;
            }
        }

        public void PrepareEditingControlForEdit(bool selectAll) {
            if (selectAll) {
                this.SelectAll();
            }
        }

        public bool RepositionEditingControlOnValueChange {
            get { return false; }
        }

        public DataGridView EditingControlDataGridView {
            get { return this.grid; }
            set { this.grid = value; }
        }

        public bool EditingControlValueChanged {
            get { return this.valueChanged; }
            set { this.valueChanged = value; }
        }

        public Cursor EditingPanelCursor {
            get { return base.Cursor; }
        }

        #endregion

        protected override void OnTextChanged(EventArgs e) {
            if (this.grid != null) {
                this.valueChanged = true;
                this.grid.NotifyCurrentCellDirty(true);
                base.OnTextChanged(e);
            }
        }
    }
}
