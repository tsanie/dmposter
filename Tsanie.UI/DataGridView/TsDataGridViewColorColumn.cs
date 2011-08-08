using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Tsanie.UI {
    public class TsDataGridViewColorColumn : DataGridViewColumn {
        public TsDataGridViewColorColumn()
            : base(new TsDataGridViewColorCell()) {
            this.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public override DataGridViewCell CellTemplate {
            get {
                return base.CellTemplate;
            }
            set {
                // Ensure that the cell used for the template is a TsDataGridViewColorCell.
                if ((value != null) && !(value is TsDataGridViewColorCell)) {
                    throw new InvalidCastException("Wrong type: " + value.GetType().Name + ", require: Tsanie.UI.TsDataGridViewColorCell");
                }
                base.CellTemplate = value;
            }
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("TsDataGridViewColorColumn { Name=");
            builder.Append(base.Name);
            builder.Append(", Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

    }
}
