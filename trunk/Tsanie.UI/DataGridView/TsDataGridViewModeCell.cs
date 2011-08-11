using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tsanie.DmPoster.Danmaku;

namespace Tsanie.UI {
    public class TsDataGridViewModeCell : DataGridViewTextBoxCell {

        private static readonly Dictionary<string, DanmakuMode> _cache;

        static TsDataGridViewModeCell() {
        }

        public override void InitializeEditingControl(
            int rowIndex,
            object initialFormattedValue,
            DataGridViewCellStyle dataGridViewCellStyle
        ) {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            ComboBox cmb = DataGridView.EditingControl as ComboBox;
            if (cmb != null)
                cmb.SelectedItem = (ModeItem)initialFormattedValue;
        }

        private static readonly Type defaultValueType = typeof(Tsanie.DmPoster.Danmaku.DanmakuMode);
        private static readonly Type defaultEditType = typeof(TsDataGridViewModeEditingControl);

        public override Type ValueType {
            get {
                return defaultValueType;
            }
        }

        public override Type EditType {
            get {
                return defaultEditType;
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
            return new ModeItem() { mode = (DanmakuMode)value }.ToString();
        }
    }
}
