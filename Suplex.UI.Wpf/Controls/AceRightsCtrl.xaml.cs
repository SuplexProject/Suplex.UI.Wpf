using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Suplex.Security.AclModel;


namespace Suplex.UI.Wpf
{
    public partial class AceRightsCtrl : UserControl
    {
        SecurityDescriptor _sd = new SecurityDescriptor();
        bool _suppressRightsEval = false;
        bool _suppressRightSelectionChanged = false;


        #region ctors, dlg handlers
        public AceRightsCtrl()
        {
            InitializeComponent();
        }
        #endregion


        #region public accessors
        public static readonly DependencyProperty AceProperty = DependencyProperty.Register( "Ace", typeof( IAccessControlEntry ), typeof( AceRightsCtrl ) );
        public IAccessControlEntry Ace
        {
            get { return GetValue( AceProperty ) as IAccessControlEntry; }
            set { SetValue( AceProperty, value ); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if( e.Property == AceProperty && Ace != null )
                CreateCheckedRights();

            base.OnPropertyChanged( e );
        }
        #endregion


        #region rights
        void CreateCheckedRights()
        {
            _suppressRightsEval = true;

            lstRight.Items.Clear();

            Type aceType = Ace.RightData.RightType;
            Array rights = Enum.GetValues( aceType );
            for( int i = rights.Length - 1; i >= 0; i-- )
            {
                CheckBox cb = new CheckBox
                {
                    Content = rights.GetValue( i ),
                };
                cb.IsChecked = (Ace.RightData.Value & (int)cb.Content) == (int)cb.Content;
                cb.Checked += Rights_CheckChanged;
                cb.Unchecked += Rights_CheckChanged;

                lstRight.Items.Add( cb );
            }

            _suppressRightsEval = false;
        }

        void lstRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( !_suppressRightSelectionChanged && ((ListBox)sender).SelectedItem != null )
            {
                CheckBox cb = (CheckBox)((ListBox)sender).SelectedItem;
                cb.IsChecked = !cb.IsChecked;
            }
        }

        void Rights_CheckChanged(object sender, RoutedEventArgs e)
        {
            if( !_suppressRightsEval )
            {
                int allowedMask = 0;
                int deniedMask = 0;

                ListBoxItem item = (ListBoxItem)lstRight.ItemContainerGenerator.ContainerFromItem( sender );
                if( item != null )
                {
                    _suppressRightSelectionChanged = true;
                    item.IsSelected = true;
                    _suppressRightSelectionChanged = false;
                }

                if( ((CheckBox)sender).IsChecked == false )
                    deniedMask |= (int)((CheckBox)sender).Content;

                foreach( CheckBox cb in lstRight.Items )
                    if( cb.IsChecked == true )
                        allowedMask |= (int)cb.Content;

                int mask = EvalRights( allowedMask, deniedMask );
                if( mask > 0 )
                    Ace.SetRight( mask.ToString() );
            }
        }

        int EvalRights(int allowedMask, int deniedMask)
        {
            Type rightType = Ace.RightData.RightType;

            IAccessControlEntry allowedAce = AccessControlEntryUtilities.MakeGenericAceFromType( rightType );
            allowedAce.Allowed = true;
            allowedAce.SetRight( allowedMask.ToString() );

            IAccessControlEntry deniedAce = AccessControlEntryUtilities.MakeGenericAceFromType( rightType );
            deniedAce.Allowed = false;
            deniedAce.SetRight( deniedMask.ToString() );

            _sd.Clear();
            _sd.Dacl.Add( allowedAce );
            _sd.Dacl.Add( deniedAce );
            _sd.Eval( rightType );

            //suppress reentrancy into this function: IsChecked=true fires CheckBox_Checked
            _suppressRightsEval = true;
            int mask = 0;
            foreach( CheckBox cb in lstRight.Items )
            {
                cb.IsChecked = _sd.Results.GetByTypeRight( rightType, (int)cb.Content ).AccessAllowed;

                if( cb.IsChecked.Value )
                    mask |= (int)cb.Content;
            }
            _suppressRightsEval = false;

            return mask;
        }
        #endregion
    }
}