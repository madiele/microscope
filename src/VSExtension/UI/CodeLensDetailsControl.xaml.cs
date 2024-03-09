#nullable enable

namespace Microscope.VSExtension.UI {
    using System.Windows.Controls;
    using System.Windows.Input;

    using Microscope.CodeAnalysis.Model;
    using Microscope.Shared;

    public partial class CodeLensDetailsControl : UserControl {
        public CodeLensDetailsControl(CodeLensDetails details) {
            InitializeComponent();
            DataContext = new DetailsData(details);
        }

        private void OnInstructionDoubleClick(object sender, MouseButtonEventArgs args) {
            //if (sender is Control c && c.DataContext is InstructionData instr) {
            //    GoToDocumentation(instr);
            //    args.Handled = true;
            //}
        }
    }
}
