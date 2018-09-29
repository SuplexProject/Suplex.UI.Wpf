using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

using Telerik.Windows.Controls;
using Telerik.Windows.Controls.TreeListView;
using Telerik.Windows.DragDrop;

using Suplex.Security.AclModel;

namespace Suplex.UI.Wpf
{
    public class SecureObjectDragDrop
    {
        const string __dragSource = "DragSource";
        const string __dragTarget = "DragTarget";
        private object _originalSource = null;
        private static Dictionary<RadTreeListView, SecureObjectDragDrop> _instances;

        static SecureObjectDragDrop()
        {
            _instances = new Dictionary<RadTreeListView, SecureObjectDragDrop>();
        }


        #region IsEnabled/Initialize/CleanUp
        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ), typeof( SecureObjectDragDrop ),
                new PropertyMetadata( new PropertyChangedCallback( OnIsEnabledPropertyChanged ) ) );

        public static void OnIsEnabledPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            SetIsEnabled( dependencyObject, (bool)e.NewValue );
        }

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue( IsEnabledProperty );
        }

        public static void SetIsEnabled(DependencyObject obj, bool isEnabled)
        {
            SecureObjectDragDrop behavior = GetAttachedBehavior( obj as RadTreeListView );
            behavior.AssociatedTreeListView = obj as RadTreeListView;

            if( isEnabled )
                behavior.Initialize();
            else
                behavior.CleanUp();

            obj.SetValue( IsEnabledProperty, isEnabled );
        }

        protected virtual void Initialize()
        {
            DragDropManager.AddDragInitializeHandler( AssociatedTreeListView, OnDragInitialize, true );
            DragDropManager.AddDropHandler( AssociatedTreeListView, OnDrop, true );
            DragDropManager.AddDragDropCompletedHandler( AssociatedTreeListView, OnDragDropCompleted, true );
            DragDropManager.AddDragOverHandler( AssociatedTreeListView, OnDragOver, true );

            AssociatedTreeListView.DataLoaded += RadTreeListView_DataLoaded;
        }

        protected virtual void CleanUp()
        {
            DragDropManager.RemoveDragInitializeHandler( AssociatedTreeListView, OnDragInitialize );
            DragDropManager.RemoveDropHandler( AssociatedTreeListView, OnDrop );
            DragDropManager.RemoveDragDropCompletedHandler( AssociatedTreeListView, OnDragDropCompleted );
            DragDropManager.RemoveDragOverHandler( AssociatedTreeListView, OnDragOver );

            AssociatedTreeListView.DataLoaded -= RadTreeListView_DataLoaded;
        }

        void RadTreeListView_DataLoaded(object sender, EventArgs e)
        {
            AssociatedTreeListView.DataLoaded -= new EventHandler<EventArgs>( RadTreeListView_DataLoaded );
            AssociatedTreeListView.ExpandAllHierarchyItems();
        }
        #endregion


        #region public props
        public IList SourceCollection { get; set; } = null;

        public RadTreeListView AssociatedTreeListView { get; set; }

        public static SecureObjectDragDrop GetAttachedBehavior(RadTreeListView gridview)
        {
            if( !_instances.ContainsKey( gridview ) )
            {
                _instances[gridview] = new SecureObjectDragDrop
                {
                    AssociatedTreeListView = gridview
                };
            }

            return _instances[gridview];
        }
        #endregion


        private void OnDragInitialize(object sender, DragInitializeEventArgs e)
        {
            var sourceRow = (e.OriginalSource as TreeListViewRow) ?? (e.OriginalSource as FrameworkElement).ParentOfType<TreeListViewRow>();
            if( sourceRow != null )
            {
                var dataObject = DragDropPayloadManager.GeneratePayload( null );

                var draggedItem = sourceRow.Item;

                DragDropPayloadManager.SetData( dataObject, __dragSource, draggedItem );
                e.Data = dataObject;

                _originalSource = sourceRow.Item;
                SourceCollection = sourceRow.ParentRow != null ? (IList)sourceRow.ParentRow.Items.SourceCollection : (IList)sourceRow.GridViewDataControl.ItemsSource;
            }
        }

        private void OnDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            object sourceItem = DragDropPayloadManager.GetDataFromObject( e.Data, __dragSource );

            TreeListViewRow destinationRow = (e.OriginalSource as TreeListViewRow) ?? (e.OriginalSource as FrameworkElement).ParentOfType<TreeListViewRow>();
            if( destinationRow != null && destinationRow.Item != sourceItem )
            {
                object targetItem = destinationRow.Item;
                e.Effects = !IsChildOf( destinationRow, _originalSource ) ? DragDropEffects.Move : DragDropEffects.None;
                if( e.Effects == DragDropEffects.Move )
                    DragDropPayloadManager.SetData( e.Data, __dragTarget, targetItem );
            }
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private bool IsChildOf(TreeListViewRow dropTarget, object originalSource)
        {
            var currentElement = dropTarget;
            while( currentElement != null )
            {
                if( currentElement.Item == originalSource )
                {
                    return true;
                }

                currentElement = currentElement.ParentRow;
            }

            return false;
        }

        private void OnDrop(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            if( e.Data != null && e.AllowedEffects != DragDropEffects.None )
            {
                SecureObject sourceItem = DragDropPayloadManager.GetDataFromObject( e.Data, __dragSource ) as SecureObject;
                SecureObject targetItem = DragDropPayloadManager.GetDataFromObject( e.Data, __dragTarget ) as SecureObject;
                IList<SecureObject> storeList = AssociatedTreeListView.DataContext as IList<SecureObject>;
                sourceItem.ChangeParent( targetItem, storeList );
            }
        }
    }
}