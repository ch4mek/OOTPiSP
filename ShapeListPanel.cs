using System;
using System.Drawing;
using System.Windows.Forms;
using OOTPiSP_LR1.Core;
using OOTPiSP_LR1.Shapes;

namespace OOTPiSP_LR1
{
    public class ShapeListPanel : UserControl
    {
        private TreeView _treeView;
        private ShapeManager _shapeManager;

        public event EventHandler<ShapeBase>? ShapeSelected;

        public ShapeListPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.BorderStyle = BorderStyle.FixedSingle;

            var titleLabel = new Label
            {
                Text = "Список фигур",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft
            };
            titleLabel.Padding = new Padding(5, 0, 0, 0);

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 15F),
                ItemHeight = 39,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true,
                HideSelection = false,
                CheckBoxes = false
            };
            _treeView.AfterSelect += TreeView_AfterSelect;

            this.Controls.Add(_treeView);
            this.Controls.Add(titleLabel);
        }

        public void SetShapeManager(ShapeManager manager)
        {
            _shapeManager = manager;
        }

        private bool _suppressSelection;

        public void RefreshList()
        {
            if (_shapeManager == null) return;

            _suppressSelection = true;
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            for (int i = 0; i < _shapeManager.ShapeCount; i++)
            {
                var shape = _shapeManager.GetShape(i);
                var node = CreateNode(shape);
                _treeView.Nodes.Add(node);
            }

            _treeView.EndUpdate();
            _suppressSelection = false;
        }

        public void SelectShape(ShapeBase shape)
        {
            _suppressSelection = true;

            if (shape == null)
            {
                _treeView.SelectedNode = null;
                _suppressSelection = false;
                return;
            }

            foreach (TreeNode node in _treeView.Nodes)
            {
                if (node.Tag == shape)
                {
                    _treeView.SelectedNode = node;
                    _suppressSelection = false;
                    return;
                }

                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag == shape)
                    {
                        node.Expand();
                        _treeView.SelectedNode = child;
                        _suppressSelection = false;
                        return;
                    }
                }
            }

            _suppressSelection = false;
        }

        private TreeNode CreateNode(ShapeBase shape)
        {
            var node = new TreeNode(shape.DisplayName)
            {
                Tag = shape
            };

            if (shape is GroupShape group)
            {
                foreach (var child in group.GetChildren())
                {
                    node.Nodes.Add(CreateNode(child));
                }
            }
            else if (shape is CompositeShape composite)
            {
                foreach (var child in composite.GetChildren())
                {
                    node.Nodes.Add(CreateNode(child));
                }
            }

            return node;
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_suppressSelection) return;

            if (e.Node?.Tag is ShapeBase shape)
            {
                ShapeSelected?.Invoke(this, shape);
            }
        }
    }
}
