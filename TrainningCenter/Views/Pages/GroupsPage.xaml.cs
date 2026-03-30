using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;
using TrainningCenter.Views.Windows.Groups;
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for GroupsPage.xaml
    /// </summary>
    public partial class GroupsPage : Page
    {
        private ObservableCollection<GroupVM> _allGroups;
        private GroupVM _selectedGroup;
        private string _searchQuery;
        private ObservableCollection<ActivityVM> _recentActivities = new ObservableCollection<ActivityVM>();
        private List<MemberVM> _currentGroupMembers = new List<MemberVM>();
        private int _selectedFilterIndex;
        public GroupsPage()
        {
            InitializeComponent();

            _allGroups = new ObservableCollection<GroupVM>();
            _recentActivities = new ObservableCollection<ActivityVM>();
            cmbSelectedFilterComboBox.SelectedIndex = 0;
            _selectedFilterIndex = 0;

            // THIS IS THE FIX: Set the source ONCE.
            RecentActivityItemsControl.ItemsSource = _recentActivities;

            // Load groups from database
            LoadAllGroupsAsync();
        }

        /// <summary>
        /// Loads all groups from database
        /// </summary>
        private async void LoadAllGroupsAsync()
        {
            try
            {
                _allGroups.Clear();
                GroupsItemsControl.Items.Clear();

                var groupsFromDb = await Group.GetAllGroups();
                foreach (var group in groupsFromDb)
                {
                    _allGroups.Add(new GroupVM
                    {
                        GroupID = group.GroupID.Value,
                        GroupName = group.GroupName,
                        SpecializationID = group.SpecializationID,
                        TrainerID = group.TrainerID,
                        CenterID = group.CenterID,
                        StartDate = group.StartDate,
                        EndDate = group.EndDate,
                        MaxTrainees = group.MaxTrainees,
                        StudentCount = group.CurrentTrainees,
                        Schedule = group.Schedule,
                        Room = group.Room,
                        StatusName = group.StatusName,
                        CreatedAt = group.CreatedAt,
                        LastUpdateAt = group.LastUpdateAt,
                        IsActive = group.IsActive,
                        BaseFee = group.BaseFee
                    });
                }

                // Apply filter
                FilterGroups();

                // Show empty state if no groups
                if (_allGroups.Count == 0)
                {
                    ShowEmptyState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Loads members for a specific group
        /// </summary>
        private async void LoadGroupMembers(int groupID)
        {
            try
            {
                if (_selectedGroup == null) return;

                //MembersItemsControl.Items.Clear();

                var loads = await Member.GetMembersList(groupID);
                _currentGroupMembers.Clear();

                foreach (var member in loads)
                {
                    _currentGroupMembers.Add(new MemberVM(member));
                }

                MembersItemsControl.ItemsSource = new ObservableCollection<MemberVM>(_currentGroupMembers);
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                    $"Error loading Member: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError("Error loading Member", ex);
            }
        }

        /// <summary>
        /// Filters groups based on search query and selected filter
        /// </summary>
        private void FilterGroups()
        {
            GroupsItemsControl.Items.Clear();
            var filteredGroups = _allGroups.AsEnumerable();

            // Apply search query filter
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                filteredGroups = filteredGroups.Where(g =>
                    g.GroupName.Contains(_searchQuery) ||
                    (g.TrainerName?.Contains(_searchQuery) ?? false));
            }

            // Apply status filter
            switch (_selectedFilterIndex)
            {
                case 1:
                    filteredGroups = filteredGroups.Where(g => g.IsActive);
                    break;

                case 2:
                    filteredGroups = filteredGroups.Where(g => !g.IsActive);
                    break;

                case 3: // My Groups (TODO: implement user-specific filter)
                    // filtered = filtered.Where(g => g.TrainerID == currentUserId);
                    break;
            }

            // Add filtered groups to ItemsControl
            foreach (var group in filteredGroups)
            {
                var groupCard = CreateGroupCard(group);
                GroupsItemsControl.Items.Add(groupCard);
            }

            // Show empty state if no results
            if (!filteredGroups.Any())
            {
                ShowEmptyState();
            }
            else
            {
                HideEmptyState();
            }
        }

        // <summary>
        /// Creates a GroupCard UI element for a group
        /// </summary>
        private Border CreateGroupCard(GroupVM group)
        {
            var card = new Border
            {
                Style = (Style)FindResource("HoverCard"),
                Padding = new Thickness(16),
                Margin = new Thickness(8, 0, 8, 12),
                Cursor = Cursors.Hand,
                Tag = group
            };

            card.MouseLeftButtonDown += (s, e) => OnGroupCardClicked(group);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconBorder = new Border
            {
                Style = (Style)FindResource("PurpleIconContainer"),
                Width = 44,
                Height = 44,
                Margin = new Thickness(0, 0, 12, 0)
            };

            var icon = new MahApps.Metro.IconPacks.PackIconMaterial
            {
                Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.AccountGroup,
                Width = 20,
                Height = 20,
                Foreground = (System.Windows.Media.Brush)FindResource("PrimaryBrush")
            };
            iconBorder.Child = icon;
            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            // Info Stack
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var nameText = new TextBlock
            {
                Text = group.GroupName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush"),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            infoStack.Children.Add(nameText);

            var countText = new TextBlock
            {
                Text = $"{group.StudentCount} members",
                FontSize = 12,
                Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 4, 0, 0)
            };
            infoStack.Children.Add(countText);

            Grid.SetColumn(infoStack, 1);
            grid.Children.Add(infoStack);

            // Status Badge
            if (group.IsActive)
            {
                var badge = new Border
                {
                    Style = (Style)FindResource("ActiveBadge"),
                    VerticalAlignment = VerticalAlignment.Top
                };

                var badgeText = new TextBlock
                {
                    Text = "ACTIVE",
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush")
                };
                badge.Child = badgeText;
                Grid.SetColumn(badge, 2);
                grid.Children.Add(badge);
            }

            card.Child = grid;
            return card;
        }

        private void ShowGroupDetails(GroupVM group)
        {
            _selectedGroup = group;

            // Hide Empty State
            HideEmptyState();
            DetailsPanel.Visibility = Visibility.Visible;

            DetailsPanel.DataContext = group;

            // Update Header
            tbSelectedGroupName.Text = group.GroupName;
            tbSelectedGroupDescription.Text = $"Trainer: {group.TrainerName} | " +
                                              $"{group.StudentCount}/{group.MaxTrainees} Students";

            // TODO: Update StatCards with real data
            // For now, they have hardcoded values in XAML

            // Load members and activity
            LoadGroupMembers(group.GroupID);
            LoadRecentActivity(group.GroupID);

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllGroupsAsync();
        }

        private void btnAddNewGroup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Add Group dialog will open here\n\nTODO: Create AddGroupDialog window",
                "Add Group", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            var dialog = new AddEditGroup();
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
                LoadAllGroupsAsync();

            // TODO: Open Add Group dialog
            // var dialog = new AddGroupDialog();
            // if (dialog.ShowDialog() == true)
            // {
            //     LoadGroups(); // Refresh after adding
            // }
        }

        private void txtSearchQueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearchQueryTextBox.Text;
            FilterGroups();
        }

        private void cmbSelectedFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedFilterIndex = cmbSelectedFilterComboBox.SelectedIndex;
            FilterGroups();
        }

        private void btnEditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null)
            {
                MessageBox.Show(
                    "Please select a group first!",
                    "No Group Selected",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                $"Edit Group dialog will open here\n\nGroup: {_selectedGroup.GroupName}",
                "Edit Group", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            var dialog = new AddEditGroup(_selectedGroup);
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (dialog.ShowDialog() == true)
                LoadAllGroupsAsync();
        }

        private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null)
            {
                MessageBox.Show("Please select a group first!", "No Group Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedGroup.GroupName}'?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // TODO: Delete from database
                    // _groupBLL.DeleteGroup(_selectedGroup.GroupID);

                    MessageBox.Show(
                        "Group deleted successfully!", 
                        "Success",
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);

                    var deletedGroup = await _selectedGroup.MapFromViewModel();
                    if (deletedGroup == null)
                    {
                        MessageBox.Show(
                            $"Error deleting group: The selected Group is null!!",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }

                    await deletedGroup.Delete(Globals.CurrentUser.UserId);
                    LoadAllGroupsAsync();
                    ShowEmptyState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error deleting group: {ex.Message}", 
                        "Error",
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
        }

        private void MemberSearchQueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 1. Get the query safely
            string query = MemberSearchQueryTextBox.Text?.Trim().ToLower() ?? "";

            // 2. If query is empty, show the full list from our "source of truth"
            if (string.IsNullOrWhiteSpace(query))
            {
                MembersItemsControl.ItemsSource = new ObservableCollection<MemberVM>(_currentGroupMembers);
                return;
            }

            // 3. Filter the private master list (_currentGroupMembers) 
            // instead of trying to cast the ItemsSource
            var filtered = _currentGroupMembers.Where(m =>
                (m.MemberName != null && m.MemberName.ToLower().Contains(query)) ||
                (m.Email != null && m.Email.ToLower().Contains(query))
            ).ToList();

            // 4. Update the UI with the filtered results
            MembersItemsControl.ItemsSource = new ObservableCollection<MemberVM>(filtered);
        }

        private void btnAddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup == null) return;

            var addMemberToGroupWindow = new AddMemberToGroupWindow(_selectedGroup.GroupID, _selectedGroup.GroupName, _selectedGroup.BaseFee);
            addMemberToGroupWindow.Owner = Application.Current.MainWindow;
            addMemberToGroupWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addMemberToGroupWindow.ShowDialog();

            LoadGroupMembers(_selectedGroup.GroupID);
        }

        private void OnGroupCardClicked(GroupVM group)
        {
            ShowGroupDetails(group);
        }

        /// <summary>
        /// Shows empty state overlay
        /// </summary>
        private void ShowEmptyState()
        {
            EmptyStateOverlay.Visibility = Visibility.Visible;
            DetailsPanel.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Hides empty state overlay
        /// </summary>
        private void HideEmptyState()
        {
            EmptyStateOverlay.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Loads recent activity for a specific group
        /// </summary>
        private async void LoadRecentActivity(int groupID)
        {
            try
            {
                // 1. Fetch the business objects from the database
                var activitiesFromDb = await Activity.GetGroupActivities(groupID);

                // 2. Clear the old list and convert the new data to ViewModels
                _recentActivities.Clear();
                if (activitiesFromDb != null && activitiesFromDb.Any())
                {
                    foreach (var act in activitiesFromDb)
                    {
                        _recentActivities.Add(new ActivityVM
                        {
                            ActivityID = act.ActivityID.Value,
                            GroupID = act.GroupID.Value,
                            ActivityType = act.ActivityType,
                            Description = act.Description,
                            Timestamp = act.Timestamp,
                            CreatedByUserID = act.CreatedByUserID.Value,
                            Username = act.Username ?? "System"
                        });
                    }
                }
                else
                {
                    // No activities found - you could add a placeholder message
                    MessageBox.Show(
                        "No recent activities found for this group.",
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                // Log the error
                EventLogger.LogError($"Failed to load recent activity for group {groupID}", ex);

                // Show user-friendly error message
                MessageBox.Show($"Failed to load recent activity: {ex.Message}",
                               "Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);

                // Clear the list on error
                _recentActivities.Clear();
            }
        }
    }
}
