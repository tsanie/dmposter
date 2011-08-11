using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Tsanie.DmPoster.Danmaku;

namespace Tsanie.UI {
    public class TsDataGridViewModeColumn : DataGridViewColumn {
        public TsDataGridViewModeColumn()
            : base(new TsDataGridViewModeCell()) {
            this.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        public override DataGridViewCell CellTemplate {
            get {
                return base.CellTemplate;
            }
            set {
                if ((value != null) && !(value is TsDataGridViewModeCell)) {
                    throw new InvalidCastException("Wrong type: " + value.GetType().FullName +
                        ", require: Tsanie.UI.TsDataGridViewModeCell");
                }
                base.CellTemplate = value;
            }
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("TsDataGridViewModeColumn { Name=");
            builder.Append(base.Name);
            builder.Append(", Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }
    }

    public class ModeItem {
        public DanmakuMode mode;
        public override string ToString() {
            return Language.Lang["DanmakuMode_" + this.mode];
        }
    }
}
