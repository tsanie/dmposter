using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tsanie.DmPoster.Danmaku;

namespace Tsanie.UI {
    public class TsDataGridViewModeEditingControl : ComboBox, IDataGridViewEditingControl {

        private DataGridView grid;
        private bool valueChanged = false;
        private int rowIndex;

        private ModeItem modeValue;

        public TsDataGridViewModeEditingControl() {
            this.TabStop = false;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Items.AddRange(new ModeItem[] {
                new ModeItem() { mode = DanmakuMode.Scroll },
                new ModeItem() { mode = DanmakuMode.BottomFixed },
                new ModeItem() { mode = DanmakuMode.TopFixed },
                new ModeItem() { mode = DanmakuMode.ReverseScroll },
                new ModeItem() { mode = DanmakuMode.Mode7 },
                new ModeItem() { mode = DanmakuMode.That_beam_of_light }
            });
        }

        #region IDataGridViewEditingControl Members

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
            this.Font = dataGridViewCellStyle.Font;
        }

        public DataGridView EditingControlDataGridView {
            get { return this.grid; }
            set { this.grid = value; }
        }

        public object EditingControlFormattedValue {
            get { return modeValue; }
            set {
                ModeItem val = value as ModeItem;
                if (val != null) {
                    this.modeValue = val;
                    this.SelectedItem = val;
                }
            }
        }

        public int EditingControlRowIndex {
            get { return rowIndex; }
            set { rowIndex = value; }
        }

        public bool EditingControlValueChanged {
            get { return this.valueChanged; }
            set { this.valueChanged = value; }
        }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) {
            // Let the ComboBox handle the keys listed.
            switch (keyData & Keys.KeyCode) {
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.PageDown:
                case Keys.PageUp:
                    return true;
                default:
                    return false;
            }
        }

        public Cursor EditingPanelCursor {
            get { return base.Cursor; }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) {
            return EditingControlFormattedValue;
        }

        public void PrepareEditingControlForEdit(bool selectAll) { }

        public bool RepositionEditingControlOnValueChange {
            get { return false; }
        }

        #endregion

        protected override void OnSelectedItemChanged(EventArgs e) {
            if (this.grid != null) {
                this.valueChanged = true;
                this.grid.NotifyCurrentCellDirty(true);
                base.OnSelectedItemChanged(e);
            }
        }
    }
}
