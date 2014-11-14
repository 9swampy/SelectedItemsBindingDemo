namespace PrimS.SelectedItemsSynchronizer
{
  using System;
  using System.Collections;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;

  /// <summary>
  /// A sync behaviour for a multiselector.
  /// </summary>
  public static class MultiSelectorBehaviours
  {
    /// <summary>
    /// The synchronized selected items.
    /// </summary>
    public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
      "SynchronizedSelectedItems", typeof(IList), typeof(MultiSelectorBehaviours), new PropertyMetadata(null, OnSynchronizedSelectedItemsChanged));

    private static readonly DependencyProperty SynchronizationManagerProperty = DependencyProperty.RegisterAttached(
      "SynchronizationManager", typeof(SynchronizationManager), typeof(MultiSelectorBehaviours), new PropertyMetadata(null));

    /// <summary>
    /// Gets the synchronized selected items.
    /// </summary>
    /// <param name="dependencyObject">The dependency object.</param>
    /// <returns>The list that is acting as the sync list.</returns>
    public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
    {
      return (IList)dependencyObject.GetValue(SynchronizedSelectedItems);
    }

    /// <summary>
    /// Sets the synchronized selected items.
    /// </summary>
    /// <param name="dependencyObject">The dependency object.</param>
    /// <param name="value">The value to be set as synchronized items.</param>
    public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
    {
      dependencyObject.SetValue(SynchronizedSelectedItems, value);
    }

    private static SynchronizationManager GetSynchronizationManager(DependencyObject dependencyObject)
    {
      return (SynchronizationManager)dependencyObject.GetValue(SynchronizationManagerProperty);
    }

    private static void SetSynchronizationManager(DependencyObject dependencyObject, SynchronizationManager value)
    {
      dependencyObject.SetValue(SynchronizationManagerProperty, value);
    }

    private static void OnSynchronizedSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != null)
      {
        SynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
        synchronizer.StopSynchronizing();

        SetSynchronizationManager(dependencyObject, null);
      }

      IList list = e.NewValue as IList;
      Selector selector = dependencyObject as Selector;

      // check that this property is an IList, and that it is being set on a ListBox
      if (list != null && selector != null)
      {
        SynchronizationManager synchronizer = GetSynchronizationManager(dependencyObject);
        if (synchronizer == null)
        {
          synchronizer = new SynchronizationManager(selector);
          SetSynchronizationManager(dependencyObject, synchronizer);
        }

        synchronizer.StartSynchronizingList();
      }
    }

    /// <summary>
    /// A synchronization manager.
    /// </summary>
    private class SynchronizationManager
    {
      private readonly Selector multiSelector;
      private TwoListSynchronizer synchronizer;

      /// <summary>
      /// Initialises a new instance of the <see cref="SynchronizationManager"/> class.
      /// </summary>
      /// <param name="selector">The selector.</param>
      internal SynchronizationManager(Selector selector)
      {
        this.multiSelector = selector;
      }

      public static IList GetSelectedItemsCollection(Selector selector)
      {
        if (selector is MultiSelector)
        {
          return (selector as MultiSelector).SelectedItems;
        }
        else if (selector is ListBox)
        {
          return (selector as ListBox).SelectedItems;
        }
        else
        {
          throw new InvalidOperationException("Target object has no SelectedItems property to bind.");
        }
      }

      /// <summary>
      /// Starts synchronizing the list.
      /// </summary>
      public void StartSynchronizingList()
      {
        IList list = GetSynchronizedSelectedItems(this.multiSelector);

        if (list != null)
        {
          this.synchronizer = new TwoListSynchronizer(GetSelectedItemsCollection(this.multiSelector), list);
          this.synchronizer.StartSynchronizing();
        }
      }

      /// <summary>
      /// Stops synchronizing the list.
      /// </summary>
      public void StopSynchronizing()
      {
        this.synchronizer.StopSynchronizing();
      }
    }
  }
}