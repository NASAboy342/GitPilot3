using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitPilot3.Models;
using GitPilot3.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit;
using System.Collections.Generic;
using LibGit2Sharp;
using GitPilot3.UserControlles;
using Avalonia.Controls.Documents;
using System.Runtime.InteropServices;
using System.IO;
using Avalonia.Threading;
using GitPilot3.Models.Graph;

namespace GitPilot3;

public partial class MainWindow : Window
{
    private readonly IFolderPicker _folderPicker;
    private readonly IGitRepositoryService _gitRepositoryService;
    public GitRepository CurrentRepository { get; set; } = new GitRepository();
    private readonly int _commitMessageRowHeight = 35;
    private readonly IUserProfileService _userProfileService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppStageService _appStageService;
    private readonly ErrorMessageHandler _errorMessageHandler;
    private FileSystemWatcher? _fileSystemWatcher;
    private IGraphComponentService _graphComponentService;
    private EventHandler OnRepositoryOpenedSuccessfully;

    public MainWindow(IUserProfileService userProfileService,
    IGitRepositoryService gitRepositoryService,
    IServiceProvider serviceProvider,
    IAppStageService appStageService,
    ErrorMessageHandler errorMessageHandler,
    LocalbrancheFlyout localbrancheFlyout,
    IGraphComponentService graphComponentService)
    {
        _userProfileService = userProfileService;
        _gitRepositoryService = gitRepositoryService;
        _serviceProvider = serviceProvider;
        _appStageService = appStageService;
        _errorMessageHandler = errorMessageHandler;
        _folderPicker = new FolderPicker(this);
        _graphComponentService = graphComponentService;
        _graphComponentService.Height = _commitMessageRowHeight;
        InitializeComponent();
        SetupWindow();
        _errorMessageHandler.ErrorMessageShown += (s, e) => AddErrorCard(e);
        _errorMessageHandler.SuccessMessageShown += (s, e) => AddSuccessCard(e);
        InitializeGitWatcher();
    }

    private async Task InitializeGitWatcher()
    {
        try
        {
            if (String.IsNullOrEmpty(CurrentRepository.Path))
                return;
            if (!CurrentRepository.IsAutoRefreshDisabled)
                return;
            _fileSystemWatcher?.Dispose();
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = CurrentRepository.Path,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _fileSystemWatcher.Changed += async (s, e) =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () => await OnRepositoryChanged());
            };
        }
        catch (Exception ex)
        {
            AddErrorCard("Failed to initialize Git watcher: " + ex.Message);
        }
    }
    private async Task OnRepositoryChanged()
    {
        try
        {
            if (CurrentRepository == null || string.IsNullOrEmpty(CurrentRepository.Path))
            {
                return;
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
            await SyncRepository();
        }
        catch (Exception ex)
        {
            _errorMessageHandler.ShowErrorMessage("Failed to refresh repository: " + ex.Message);
        }
    }

    private async Task SyncRepository()
    {
        if (CurrentRepository == null || string.IsNullOrEmpty(CurrentRepository.Path))
            throw new InvalidOperationException("No repository is currently loaded.");

        var localBranches = new List<GitBranch>();
        var remoteBranches = new List<GitBranch>();

        var tasks = new List<Task>()
        {
            Task.Run(async () => {localBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);}),
            Task.Run(async () => {remoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(CurrentRepository.Path);}),
            Task.Run(async () => {CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(CurrentRepository.Path);}),
        };

        await Task.WhenAll(tasks);

        UpdateLocalBranches(localBranches);
        UpdateRemoteBranches(remoteBranches);
        UpdateCurrentRepositoryDisplay();

        if (CurrentRepository.CommitDetail != null && !string.IsNullOrEmpty(CurrentRepository.CommitDetail.Commit.Sha))
        {
            CurrentRepository.CommitDetail = await _gitRepositoryService.GetCommitDetailsAsync(CurrentRepository.Path, CurrentRepository.CommitDetail.Commit);
            UpdateFileChangesView();
        }

        _appStageService.SaveCurrentRepository(CurrentRepository);
    }

    private void UpdateRemoteBranches(List<GitBranch> remoteBranches)
    {
        foreach (var updatedBranch in remoteBranches)
        {
            var existingBranch = CurrentRepository.RemoteBranches.FirstOrDefault(b => b.Name == updatedBranch.Name);
            if (existingBranch != null)
            {
                existingBranch.InComming = updatedBranch.InComming;
                existingBranch.OutGoing = updatedBranch.OutGoing;
            }
            else
            {
                CurrentRepository.RemoteBranches.Add(updatedBranch);
            }
        }
        foreach (var existingBranch in CurrentRepository.RemoteBranches.ToList())
        {
            if (!remoteBranches.Any(b => b.Name == existingBranch.Name))
            {
                CurrentRepository.RemoteBranches.Remove(existingBranch);
            }
        }
    }

    private void UpdateLocalBranches(List<GitBranch> newLocalBranchesData)
    {
        foreach (var updatedBranch in newLocalBranchesData)
        {
            var existingBranch = CurrentRepository.LocalBranches.FirstOrDefault(b => b.Name == updatedBranch.Name);
            if (existingBranch != null)
            {
                existingBranch.InComming = updatedBranch.InComming;
                existingBranch.OutGoing = updatedBranch.OutGoing;
            }
            else
            {
                CurrentRepository.LocalBranches.Add(updatedBranch);
            }
        }
        foreach (var existingBranch in CurrentRepository.LocalBranches.ToList())
        {
            if (!newLocalBranchesData.Any(b => b.Name == existingBranch.Name))
            {
                CurrentRepository.LocalBranches.Remove(existingBranch);
            }
        }
    }

    private void SetupWindow()
    {
        this.WindowState = WindowState.Maximized;

        LoadProfileInfoDisplay();
        LoadLastOpentedRepository();
    }

    private void AddSuccessCard(string message)
    {
        var errorStackPanel = this.FindControl<StackPanel>("ErrorStackPanel");
        if (errorStackPanel == null)
            return;

        var successCard = new SuccessCard(message);
        successCard.CloseSuccessCardClicked += (s, e) =>
        {
            errorStackPanel.Children.Remove(successCard);
        };
        errorStackPanel.Children.Add(successCard);
    }

    private void AddErrorCard(string message)
    {
        var errorStackPanel = this.FindControl<StackPanel>("ErrorStackPanel");
        if (errorStackPanel == null)
            return;

        var errorCard = new ErrorCard(message);
        errorCard.CloseErrorCardClicked += (s, e) =>
        {
            errorStackPanel.Children.Remove(errorCard);
        };
        errorStackPanel.Children.Add(errorCard);
    }
    private void LoadLastOpentedRepository()
    {
        try
        {
            var lastRepository = _appStageService.GetCurrentRepository();
            if (lastRepository == null)
                return;
            CurrentRepository = lastRepository;
            UpdateCurrentRepositoryDisplay();
            UpdateRepositoryComboBox();
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }

    private void UpdateRepositoryComboBox()
    {
        var openedRepositories = _appStageService.GetAllOpenedRepositories().OrderByDescending(r => r.LastOpened).ToList();

        if (OpenedRepositoriesPanel == null) return;

        OpenedRepositoriesPanel.Children.Clear();

        var searchableSelection = new SearchableSelection
        {
            Width = 200,
            Height = 25,
            Items = openedRepositories.OrderByDescending(r => r.LastOpened).Select(r => r.Name).ToList(),
            SelectedItem = CurrentRepository.Name
        };
        OnNewRepoIsOpent(searchableSelection);
        OnSelectedRepoChanged(openedRepositories, searchableSelection);

        OpenedRepositoriesPanel.Children.Add(searchableSelection);
    }

    private void OnNewRepoIsOpent(SearchableSelection searchableSelection)
    {

        OnRepositoryOpenedSuccessfully += async (s, e) =>
        {
            try
            {
                var openedRepositories = _appStageService.GetAllOpenedRepositories().OrderByDescending(r => r.LastOpened).ToList();
                searchableSelection.SelectedItem = CurrentRepository.Name;
                searchableSelection.Items = openedRepositories.OrderByDescending(r => r.LastOpened).Select(r => r.Name).ToList();
            }
            catch (Exception ex)
            {
                AddErrorCard(ex.Message);
            }
        };
    }

    private void OnSelectedRepoChanged(List<OpenedRepository> openedRepositories, SearchableSelection searchableSelection)
    {
        searchableSelection.SelectedItemChanged += async (s, e) =>
        {
            try
            {
                var selectedRepoName = searchableSelection.SelectedItem;
                var selectedOpenedRepo = openedRepositories.FirstOrDefault(r => r.Name.Equals(selectedRepoName, StringComparison.OrdinalIgnoreCase));
                if (selectedOpenedRepo == null)
                    return;
                CurrentRepository = await _gitRepositoryService.LoadRepositoryAsync(selectedOpenedRepo.Path);
                CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(selectedOpenedRepo.Path);
                CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(selectedOpenedRepo.Path);
                CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(selectedOpenedRepo.Path);
                UpdateCurrentRepositoryDisplay();
                _appStageService.SaveCurrentRepository(CurrentRepository);
                InitializeGitWatcher();
                AddSuccessCard($"Switched to repository: {CurrentRepository.Name}.");
            }
            catch (Exception ex)
            {
                _errorMessageHandler.ShowErrorMessage("Failed to switch repository: " + ex.Message);
                return;
            }
        };
    }

    private async Task LoadProfileInfoDisplay()
    {
        try
        {
            var profileButton = this.FindControl<Button>("ProfileButton");
            if (profileButton == null)
                return;
            var currentProfile = await _userProfileService.GetCurrentUserProfileAsync();
            if (currentProfile == null)
                return;
            var stackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };
            var profileIcon = new Border
            {
                Width = 24,
                Height = 24,
                Background = new Avalonia.Media.SolidColorBrush(currentProfile.ToAvaloniaColor()),
                CornerRadius = new Avalonia.CornerRadius(12),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            var profileIconLetter = new TextBlock
            {
                Text = currentProfile.Username.Length > 0 ? currentProfile.Username[0].ToString().ToUpper() : "?",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Avalonia.Media.Brushes.White
            };
            profileIcon.Child = profileIconLetter;
            stackPanel.Children.Add(profileIcon);
            var username = new TextBlock
            {
                Text = currentProfile.Username,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            stackPanel.Children.Add(username);
            profileButton.Content = stackPanel;
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }


    private async void OnOpenRepository(object sender, RoutedEventArgs e)
    {
        try
        {
            var repositoryPath = await _folderPicker.ShowDialogAsync();
            if (string.IsNullOrEmpty(repositoryPath))
                return;
            CurrentRepository = await _gitRepositoryService.LoadRepositoryAsync(repositoryPath);
            CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(repositoryPath);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(repositoryPath);
            CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(repositoryPath);
            UpdateCurrentRepositoryDisplay();
            _appStageService.SaveCurrentRepository(CurrentRepository);
            InitializeGitWatcher();
            OnRepositoryOpenedSuccessfully?.Invoke(this, EventArgs.Empty);
            AddSuccessCard($"Repository: {CurrentRepository.Name} loaded successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }

    private void UpdateCurrentRepositoryDisplay()
    {
        RepoNameTextBlock.Text = CurrentRepository.Name;
        RepoPathTextBlock.Text = CurrentRepository.Path;
        UpdateLocalBranchesTreeView();
        UpdateRemoteBranchesTreeView();
        UpdateCurrentBranchNameDisplay();
        UpdateCommitsInfoView();
    }

    private void UpdateCommitsInfoView()
    {
        UpdateCommitsMessageView();
        UpdateCommitsGraphView();
        UpdateCommitsDateView();
        UpdateCommitsBranchView();
    }

    private void UpdateCommitsBranchView()
    {
        var commitsBranchStackPanel = this.FindControl<StackPanel>("BranchNamesHeaderStackPanel");
        if (commitsBranchStackPanel == null)
            return;
        commitsBranchStackPanel.Children.Clear();
        commitsBranchStackPanel.Children.Add(GetGraphHeaderItem("Branches"));
        foreach (var commit in CurrentRepository.Commits)
        {
            var commitBranch = GetCommitBranchByName(commit.BranchName);
            var commitBranchItem = new Border
            {
                Height = _commitMessageRowHeight,
                Child = new Border
                {
                    Height = _commitMessageRowHeight - 5,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 5, 0),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Background = string.IsNullOrEmpty(commit.BranchName) ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(commitBranch.ToAvaloniaColor()),
                    Child = new TextBlock
                    {
                        Text = commit.BranchName,
                        FontSize = 12,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Avalonia.Thickness(5, 0, 0, 0)
                    }
                }
            };
            commitsBranchStackPanel.Children.Add(commitBranchItem);
        }
    }

    private GitBranch GetCommitBranchByName(string branchName)
    {
        return CurrentRepository.LocalBranches.FirstOrDefault(b => b.Name == branchName)
            ?? CurrentRepository.RemoteBranches.FirstOrDefault(b => b.Name == branchName)
            ?? new GitBranch();
    }

    private void UpdateCommitsDateView()
    {
        var commitsDateStackPanel = this.FindControl<StackPanel>("CommitDateHeaderStackPanel");
        if (commitsDateStackPanel == null)
            return;
        commitsDateStackPanel.Children.Clear();
        commitsDateStackPanel.Children.Add(GetGraphHeaderItem("Dates"));
        foreach (var commit in CurrentRepository.Commits)
        {
            var commitDateItem = new Border
            {
                Height = _commitMessageRowHeight,
                Child = new TextBlock
                {
                    Text = commit.AuthorDate.ToString("yyyy-MM-dd"),
                    FontSize = 12,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 0, 0)
                }
            };
            commitsDateStackPanel.Children.Add(commitDateItem);
        }
    }

    private void UpdateCommitsGraphView()
    {
        var commitsGraphStackPanel = this.FindControl<StackPanel>("GitGraphColumn");
        if (commitsGraphStackPanel == null)
            return;
        commitsGraphStackPanel.Children.Clear();
        commitsGraphStackPanel.Children.Add(GetGraphHeaderItem("Graph"));
        DrawGraphs(commitsGraphStackPanel);
    }

    private void DrawGraphs(StackPanel commitsGraphStackPanel)
    {
        var branchDrawBuffers = new List<BranchDrawBuffer>();
        var mergePointBuffers = new List<MergePointBuffer>();
        foreach (var commit in CurrentRepository.Commits)
        {
            var isAMergeCommit = commit.ParentShas.Count > 1;
            var newCanvas = new Canvas
            {
                Height = _commitMessageRowHeight,
            };
            var isAlreadyDrawnCommitPoint = false;
            var drawnCommitPointIndex = 0;
            var pendingRemoveFromBranchBuffers = new List<BranchDrawBuffer>();
            foreach (var branchDrawBuffer in branchDrawBuffers)
            {
                if (IsFoundParentCommit(commit, branchDrawBuffer))
                {
                    if (isAlreadyDrawnCommitPoint)
                    {
                        newCanvas.Children.Add(_graphComponentService.GetCheckOutCurveLineCanvas(branchDrawBuffers.IndexOf(branchDrawBuffer), branchDrawBuffer.Color));
                        DrawCheckoutLineFromParent(branchDrawBuffers, newCanvas, drawnCommitPointIndex, branchDrawBuffer);
                        pendingRemoveFromBranchBuffers.Add(branchDrawBuffer);
                    }
                    else
                    {
                        var relativeColor = GetBranchColorOfCommit(commit, CurrentRepository);
                        newCanvas.Children.Add(_graphComponentService.GetUpperConnectorLineCanvas(branchDrawBuffers.IndexOf(branchDrawBuffer), relativeColor ?? branchDrawBuffer.Color));
                        newCanvas.Children.Add(_graphComponentService.GetCommitPointCanvas(branchDrawBuffers.IndexOf(branchDrawBuffer), isAMergeCommit, relativeColor ?? branchDrawBuffer.Color));
                        newCanvas.Children.Add(_graphComponentService.GetLowerConnectorLineCanvas(branchDrawBuffers.IndexOf(branchDrawBuffer), relativeColor ?? branchDrawBuffer.Color));
                        branchDrawBuffer.Sha = commit.Sha;
                        branchDrawBuffer.ParentShas = commit.ParentShas;
                        branchDrawBuffer.Color = relativeColor ?? branchDrawBuffer.Color;
                        isAlreadyDrawnCommitPoint = true;
                        drawnCommitPointIndex = branchDrawBuffers.IndexOf(branchDrawBuffer);
                    }
                }
                else
                {
                    newCanvas.Children.Add(_graphComponentService.GetBranchLineCanvas(branchDrawBuffers.IndexOf(branchDrawBuffer), branchDrawBuffer.Color));
                }
            }
            if (!isAlreadyDrawnCommitPoint)
            {
                var relativeColor = GetBranchColorOfCommit(commit, CurrentRepository) ?? new RGBColor(0, 0, 0);
                newCanvas.Children.Add(_graphComponentService.GetCommitPointCanvas(branchDrawBuffers.Count, isAMergeCommit, relativeColor));
                newCanvas.Children.Add(_graphComponentService.GetLowerConnectorLineCanvas(branchDrawBuffers.Count, relativeColor));
                branchDrawBuffers.Add(new BranchDrawBuffer { Sha = commit.Sha, ParentShas = commit.ParentShas, Color = relativeColor });
            }

            if (IsFountParentSource(mergePointBuffers, commit, out List<MergePointBuffer> foundMergePoints))
            {
                DrawMerge(branchDrawBuffers, mergePointBuffers, commit, newCanvas, foundMergePoints);
            }
            mergePointBuffers.ForEach(mp => mp.RowsAwayFromMergePoint++);

            if (isAMergeCommit)
            {
                SaveMergePoint(branchDrawBuffers, mergePointBuffers, commit);
            }
            if (pendingRemoveFromBranchBuffers.Any())
            {
                foreach (var buffer in pendingRemoveFromBranchBuffers)
                {
                    branchDrawBuffers.Remove(buffer);
                }
            }


            commitsGraphStackPanel.Children.Add(newCanvas);
        }
    }

    private RGBColor GetBranchColorOfCommit(GitCommit commit, GitRepository currentRepository)
    {
        var branch = currentRepository.LocalBranches.FirstOrDefault(b => b.Name.Equals(commit.BranchName))
            ?? currentRepository.RemoteBranches.FirstOrDefault(b => b.Name.Equals(commit.BranchName));
        if (branch != null)
        {
            return branch.Color;
        }
        return null;
    }

    private void DrawMerge(List<BranchDrawBuffer> branchDrawBuffers, List<MergePointBuffer> mergePointBuffers, GitCommit commit, Canvas newCanvas, List<MergePointBuffer> foundMergePoints)
    {
        foreach (var foundMergePoint in foundMergePoints)
        {
            for (int i = 1; i <= foundMergePoint.RowsAwayFromMergePoint; i++)
            {
                if (i == foundMergePoint.RowsAwayFromMergePoint)
                {
                    var currentBranchFromBuffer = branchDrawBuffers.First(b => b.Sha.Equals(commit.Sha));
                    newCanvas.Children.Add(_graphComponentService.GetUpperConnectorLineCanvas(branchDrawBuffers.IndexOf(currentBranchFromBuffer), currentBranchFromBuffer.Color));
                    newCanvas.Children.Add(_graphComponentService.GetMergeCurveLineOnOtherRowCanvas(branchDrawBuffers.IndexOf(currentBranchFromBuffer), i * -1, foundMergePoint.MergeToIndex, currentBranchFromBuffer.Color));
                    DrawHorizontalLineToMergeTarget(branchDrawBuffers, newCanvas, foundMergePoint, i, currentBranchFromBuffer);
                }
                else
                {
                    var currentBranchFromBuffer = branchDrawBuffers.First(b => b.Sha.Equals(commit.Sha));
                    newCanvas.Children.Add(_graphComponentService.GetBranchLineCanvasOnOtherRow(branchDrawBuffers.IndexOf(currentBranchFromBuffer), i * -1, currentBranchFromBuffer.Color));
                }
            }
            mergePointBuffers.Remove(foundMergePoint);
        }
    }

    private void DrawHorizontalLineToMergeTarget(List<BranchDrawBuffer> branchDrawBuffers, Canvas newCanvas, MergePointBuffer foundMergePoint, int i, BranchDrawBuffer currentBranchFromBuffer)
    {
        var isMergeToRight = branchDrawBuffers.IndexOf(currentBranchFromBuffer) < foundMergePoint.MergeToIndex;
        if (isMergeToRight)
        {
            for (int stepToMergeTarget = branchDrawBuffers.IndexOf(currentBranchFromBuffer) + 1; stepToMergeTarget < foundMergePoint.MergeToIndex; stepToMergeTarget++)
            {
                newCanvas.Children.Add(_graphComponentService.HorizontalLineToRightOnOtherRowCanvas(stepToMergeTarget, i * -1, currentBranchFromBuffer.Color));
            }
        }
        else
        {
            for (int stepToMergeTarget = branchDrawBuffers.IndexOf(currentBranchFromBuffer) - 1; stepToMergeTarget > foundMergePoint.MergeToIndex; stepToMergeTarget--)
            {
                newCanvas.Children.Add(_graphComponentService.HorizontalLineToLeftOnOtherRowCanvas(stepToMergeTarget, i * -1, currentBranchFromBuffer.Color));
            }
        }
    }

    private bool IsFountParentSource(List<MergePointBuffer> mergePointBuffers, GitCommit commit, out List<MergePointBuffer> foundMergePoints)
    {
        foundMergePoints = new List<MergePointBuffer>();
        foreach (var mergePoint in mergePointBuffers)
        {
            if (mergePoint.MergeFromSha.Equals(commit.Sha))
            {
                foundMergePoints.Add(mergePoint);
            }
        }
        return foundMergePoints.Count > 0;
    }

    private static void SaveMergePoint(List<BranchDrawBuffer> branchDrawBuffers, List<MergePointBuffer> mergePointBuffers, GitCommit commit)
    {
        for (var i = 1; i < commit.ParentShas.Count; i++)
        {
            var targetBranchDrawBuffer = branchDrawBuffers.First(x => x.Sha.Equals(commit.Sha));
            mergePointBuffers.Add(new MergePointBuffer
            {
                MergeFromSha = commit.ParentShas[i],
                MergeToIndex = branchDrawBuffers.IndexOf(targetBranchDrawBuffer),
                RowsAwayFromMergePoint = 1
            });
        }
    }

    private void DrawCheckoutLineFromParent(List<BranchDrawBuffer> branchDrawBuffers, Canvas newCanvas, int drawnCommitPointIndex, BranchDrawBuffer branchDrawBuffer)
    {
        var currentIndex = branchDrawBuffers.IndexOf(branchDrawBuffer);
        for (var i = currentIndex - 1; i > drawnCommitPointIndex; i--)
        {
            newCanvas.Children.Add(_graphComponentService.HorizontalLineToLeftCanvas(i, branchDrawBuffer.Color));
        }
    }

    private static bool IsFoundParentCommit(GitCommit commit, BranchDrawBuffer branchDrawBuffer)
    {
        if (branchDrawBuffer.ParentShas.Count == 0)
            return false;
        return commit.Sha.Equals(branchDrawBuffer.ParentShas[0]);
    }

    private void UpdateCommitsMessageView()
    {
        var commitsMessageStackPanel = this.FindControl<StackPanel>("CommitMessageHeaderStackPanel");
        if (commitsMessageStackPanel == null)
            return;
        commitsMessageStackPanel.Children.Clear();
        commitsMessageStackPanel.Children.Add(GetGraphHeaderItem("Commit Messages"));
        foreach (var commit in CurrentRepository.Commits)
        {
            var parentCommits = $" parents: {string.Join(", ", commit.ParentShas.Select(p => p.Substring(0, 7)))}";
            var commitMessageItem = new Border
            {
                Height = _commitMessageRowHeight,
                Child = new TextBlock
                {
                    Text = commit.Message + (commit.IsWorkInProgress ? $"   ✏️{commit.ChangedFilesCount}" : "") + parentCommits,
                    FontSize = 12,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(5, 0, 0, 0)
                },
                Classes = { "commitMessageBorder" }

            };
            commitMessageItem.Tapped += async (sender, e) =>
            {
                await ShowCommitDetails(commit);
            };
            commitsMessageStackPanel.Children.Add(commitMessageItem);
        }
    }

    private async Task ShowCommitDetails(GitCommit commit)
    {
        CurrentRepository.CommitDetail = await _gitRepositoryService.GetCommitDetailsAsync(CurrentRepository.Path, commit);
        UpdateFileChangesView();
    }

    private void UpdateFileChangesView()
    {
        UpdateCommitDetailsHeaderView();
        UpdateCommitDetailMessagePanel();
        UpdateFileChangesHeaderView();
        UpdateFileChangesStackPanel();
        UpdateStagedFileChangesStackPanel();
        SetStagedFileChangesVisibility(CurrentRepository.CommitDetail.Commit.IsWorkInProgress);
    }

    private void UpdateStagedFileChangesStackPanel()
    {
        if (CurrentRepository.CommitDetail == null || !CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            return;

        var stagedFileChangesStackPanel = this.FindControl<StackPanel>("StagedFileChangesStackPanel");
        if (stagedFileChangesStackPanel == null)
            return;

        stagedFileChangesStackPanel.Children.Clear();

        var stagedHeader = new TextBlock
        {
            Text = "Staged Changes",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(5, 10, 0, 5)
        };
        stagedFileChangesStackPanel.Children.Add(stagedHeader);

        foreach (var fileChange in CurrentRepository.CommitDetail.FilesChanged.Where(f => f.IsStaged))
        {
            var fileChangeItem = new DockPanel
            {
                Height = 30,
                Classes = { "fileChangeBorder" },
            };

            var fileInfo = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,

                Children =
                    {
                        new TextBlock
                        {
                            Text = GetChangeTypeSymbol(fileChange.ChangeType),
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = fileChange.FileName,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"+{fileChange.Additions}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.Green,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"-{fileChange.Deletions}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.OrangeRed,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        }
                    }
            };
            DockPanel.SetDock(fileInfo, Dock.Left);
            fileInfo.Tapped += async (sender, e) =>
            {
                await ShowFileChangeDetail(fileChange);
            };

            var unstageFileButton = new Button
            {
                Content = "−",
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Height = 20,
                Width = 20,
                Padding = new Avalonia.Thickness(5, 0, 5, 0),
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };

            unstageFileButton.Click += async (sender, e) =>
            {
                await UnstageFileChange(fileChange);
            };
            DockPanel.SetDock(unstageFileButton, Dock.Right);

            fileChangeItem.Children.Add(unstageFileButton);
            fileChangeItem.Children.Add(fileInfo);
            stagedFileChangesStackPanel.Children.Add(fileChangeItem);
        }
    }

    private void SetStagedFileChangesVisibility(bool visible)
    {
        var fileChangesSplitter = this.FindControl<GridSplitter>("FileChangesSplitter");
        var stagedFileChangesScrollViewer = this.FindControl<ScrollViewer>("StagedFileChangesScrollViewer");
        var commitInputScrollViewer = this.FindControl<StackPanel>("CommitInputScrollViewer");

        if (stagedFileChangesScrollViewer != null && fileChangesSplitter != null && commitInputScrollViewer != null)
        {
            // fileChangesSplitter.IsVisible = visible;
            stagedFileChangesScrollViewer.IsVisible = visible;
            commitInputScrollViewer.IsVisible = visible;
        }
    }

    private async Task UnstageFileChange(GitCommitFileChange fileChange)
    {
        if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
        {
            await _gitRepositoryService.UnStageFilesAsync(CurrentRepository.Path, new List<string> { fileChange.FilePath });
            await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
        }
    }

    private void UpdateFileChangesStackPanel()
    {
        var fileChangesStackPanel = this.FindControl<StackPanel>("FileChangesStackPanel");
        if (fileChangesStackPanel == null)
            return;

        fileChangesStackPanel.Children.Clear();

        var stagedHeader = new TextBlock
        {
            Text = "Changes",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(5, 10, 0, 5)
        };
        fileChangesStackPanel.Children.Add(stagedHeader);

        var fileChanges = CurrentRepository.CommitDetail.FilesChanged.Where(f => !CurrentRepository.CommitDetail.Commit.IsWorkInProgress || !f.IsStaged).ToList();

        foreach (var fileChange in fileChanges)
        {
            var fileChangeItem = new DockPanel
            {
                Height = 30,
            };

            var fileInfo = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Classes = { "fileChangeBorder" },
                Children =
                    {
                        new TextBlock
                        {
                            Text = GetChangeTypeSymbol(fileChange.ChangeType),
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = fileChange.FileName,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"+{fileChange.Additions}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.Green,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = $"-{fileChange.Deletions}",
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.OrangeRed,
                            Margin = new Avalonia.Thickness(5, 0, 0, 0)
                        }
                    }
            };
            DockPanel.SetDock(fileInfo, Dock.Left);
            fileInfo.Tapped += async (sender, e) =>
            {
                await ShowFileChangeDetail(fileChange);
            };
            fileChangeItem.Children.Add(fileInfo);

            if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            {
                var stagedFileButton = GetStageFileButton(fileChange);
                fileChangeItem.Children.Add(stagedFileButton);
                var discardFileButton = GetDiscardFileButton(fileChange);
                fileChangeItem.Children.Add(discardFileButton);
            }

            fileChangesStackPanel.Children.Add(fileChangeItem);
        }
    }

    private Button GetDiscardFileButton(GitCommitFileChange fileChange)
    {
        var discardFileButton = new Button
        {
            Content = "-",
            Foreground = Brushes.White,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Height = 20,
            Width = 20,
            Padding = new Avalonia.Thickness(5, 0, 5, 0),
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };
        DockPanel.SetDock(discardFileButton, Dock.Right);
        discardFileButton.Click += async (sender, e) =>
        {
            var newWindow = new Window
            {
                Title = "Clone Repository",
                Width = 500,
                Height = 100,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            var commonConfirmation = new CommonConfirmation()
            {
                Message = $"Are you sure you want to discard changes in file: {fileChange.FileName}?",
            };
            newWindow.Content = commonConfirmation;
            commonConfirmation.OnCancelClicked += (s, e) =>
            {
                newWindow.Close();
            };
            commonConfirmation.OnYesClicked += async (s, e) =>
            {
                await DiscardFileAsync(new List<string> { fileChange.FilePath });
                newWindow.Close();
            };
            newWindow.ShowDialog(this);
        };
        return discardFileButton;
    }

    private async Task DiscardFileAsync(List<string> unstageFilePaths)
    {
        try
        {
            if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            {
                await _gitRepositoryService.DiscardFilesAsync(CurrentRepository.Path, unstageFilePaths);
                await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
            }
        }
        catch (Exception ex)
        {
            AddErrorCard("Failed to discard files: " + ex.Message);
            return;
        }
    }

    private Button GetStageFileButton(GitCommitFileChange fileChange)
    {
        var stagedFileButton = new Button
        {
            Content = "+",
            Foreground = Brushes.White,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Height = 20,
            Width = 20,
            Padding = new Avalonia.Thickness(5, 0, 5, 0),
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };
        DockPanel.SetDock(stagedFileButton, Dock.Right);
        stagedFileButton.Click += async (sender, e) =>
        {
            await StageFilesAsync(new List<string> { fileChange.FilePath });
        };
        return stagedFileButton;
    }

    private async Task StageFilesAsync(List<string> unstageFilePaths)
    {
        try
        {
            if (CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            {
                await _gitRepositoryService.StageFilesAsync(CurrentRepository.Path, unstageFilePaths);
                await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
            }
        }
        catch (Exception ex)
        {
            AddErrorCard("Failed to stage files: " + ex.Message);
            return;
        }

    }

    private async Task ShowFileChangeDetail(GitCommitFileChange fileChange)
    {
        var fileContentScrollViewer = this.FindControl<ScrollViewer>("FileContentScrollViewer");
        var commitGraphScrollViewer = this.FindControl<ScrollViewer>("CommitGraphScrollViewer");

        if (fileContentScrollViewer == null)
            return;

        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };

        stackPanel.Children.Add(GetFileContentHeader(fileChange, fileContentScrollViewer, commitGraphScrollViewer));

        stackPanel.Children.Add(GetFileContentView(fileChange));
        fileContentScrollViewer.Content = stackPanel;
        fileContentScrollViewer.IsVisible = true;

        if (commitGraphScrollViewer != null)
        {
            commitGraphScrollViewer.IsVisible = false;
        }
    }

    private Control GetFileContentView(GitCommitFileChange fileChange)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Avalonia.Thickness(5)
        };

        var lines = fileChange.DiffContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var selectableTextBlock = new SelectableTextBlock
            {
                FontFamily = new FontFamily("Consolas, 'Courier New', monospace"),
                FontSize = 16,
            };
        
        foreach (var line in lines)
        {
            var lineColor = Brushes.White;
            var backgroundColor = Brushes.Transparent;
            if (line.StartsWith("+"))
            {
                lineColor = Brushes.LightGreen;
                backgroundColor = (IImmutableSolidColorBrush)new SolidColorBrush(Color.FromRgb(30, 50, 30), 0.5).ToImmutable();
                
            }
            else if (line.StartsWith("-"))
            {
                lineColor = Brushes.IndianRed;
                backgroundColor = (IImmutableSolidColorBrush)new SolidColorBrush(Color.FromRgb(50, 30, 30), 0.5).ToImmutable();
            }
            
            selectableTextBlock.Inlines.Add(new Run
            {
                Text = line + Environment.NewLine,
                Foreground = lineColor,
                Background = backgroundColor,
            });
            
        }

        stackPanel.Children.Add(selectableTextBlock);

        return stackPanel;
    }

    private Control GetFileContentHeader(GitCommitFileChange fileChange, ScrollViewer fileContentScrollViewer, ScrollViewer? commitGraphScrollViewer)
    {
        var headerStackPanel = new DockPanel
        {
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
        };

        var fileNameTextBlock = new TextBlock
        {
            Text = fileChange.FileName,
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Avalonia.Thickness(5, 0, 0, 0)
        };
        DockPanel.SetDock(fileNameTextBlock, Dock.Left);

        var backButton = new Button
        {
            Content = "✖︎",
            Padding = new Avalonia.Thickness(5, 0, 5, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 5, 0)
        };
        DockPanel.SetDock(backButton, Dock.Right);

        backButton.Click += (sender, e) =>
        {
            fileContentScrollViewer.IsVisible = false;
            if (commitGraphScrollViewer != null)
            {
                commitGraphScrollViewer.IsVisible = true;
            }
        };

        headerStackPanel.Children.Add(fileNameTextBlock);
        headerStackPanel.Children.Add(backButton);

        return headerStackPanel;
    }

    private string GetChangeTypeSymbol(string changeType)
    {
        switch (changeType)
        {
            case "Added":
                return "➕";
            case "Modified":
                return "✏️";
            case "Deleted":
                return "❌";
            case "Renamed":
                return "🔄";
            case "Copied":
                return "📋";
            default:
                return "❓";
        }
    }

    private void UpdateFileChangesHeaderView()
    {
        var fileChangesHeader = this.FindControl<Border>("FileChangesHeader");
        if (fileChangesHeader == null)
            return;

        var profileStackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };

        profileStackPanel.Children.Add(GetProfileIcon());
        profileStackPanel.Children.Add(GetProfileInfo());
        fileChangesHeader.Child = profileStackPanel;
    }

    private Control GetProfileInfo()
    {
        var profileInfoStackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        profileInfoStackPanel.Children.Add(new TextBlock
        {
            Text = CurrentRepository.CommitDetail.Commit.AuthorName,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = Brushes.White,
            FontSize = 14,
        });

        profileInfoStackPanel.Children.Add(new TextBlock
        {
            Text = "authored: " + CurrentRepository.CommitDetail.Commit.AuthorDate.ToString("MM/dd/yyyy @ HH:mm tt"),
            Foreground = Brushes.Gray,
        });

        return profileInfoStackPanel;
    }

    private Control GetProfileIcon()
    {
        var profileIconBlock = new Border
        {
            Width = 30,
            Height = 30,
            Background = Brushes.DodgerBlue,
            CornerRadius = new Avalonia.CornerRadius(15),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var profileLetter = CurrentRepository.CommitDetail.Commit.AuthorName;
        profileLetter = string.IsNullOrEmpty(profileLetter) ? "?" : profileLetter.Substring(0, 1).ToUpper();
        profileIconBlock.Child = new TextBlock
        {
            Text = profileLetter,
            Foreground = Brushes.White,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 16,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        return profileIconBlock;
    }

    private void UpdateCommitDetailMessagePanel()
    {
        var commitDetailMessagePanel = this.FindControl<StackPanel>("CommitDetailMessagePanel");
        if (commitDetailMessagePanel == null)
            return;
        commitDetailMessagePanel.Children.Clear();
        commitDetailMessagePanel.Children.Add(new TextBlock
        {
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Text = CurrentRepository.CommitDetail.Commit.Message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });
        commitDetailMessagePanel.Children.Add(new TextBlock
        {
            FontSize = 12,
            Foreground = Brushes.Gray,
            Text = CurrentRepository.CommitDetail.Commit.Description,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        });

    }

    private void UpdateCommitDetailsHeaderView()
    {
        var commitDetailsHeader = this.FindControl<TextBlock>("CommitDetailsHeader");
        if (commitDetailsHeader == null)
            return;

        var commitSha = CurrentRepository.CommitDetail?.Commit.Sha ?? "";
        commitSha = commitSha.Length > 7 ? commitSha.Substring(0, 7) : commitSha;

        commitDetailsHeader.Text = "commit: " + commitSha;
    }

    private Border? GetGraphHeaderItem(string headerName)
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
            Height = 25,
            Child = new TextBlock
            {
                Text = headerName,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5, 0, 0, 0)
            }
        };
    }

    private void UpdateCurrentBranchNameDisplay()
    {
        var currentBranch = CurrentRepository.LocalBranches.FirstOrDefault(b => b.IsCurrent);
        BranchCurrentTextBlock.Text = currentBranch != null ? $"Branch: {currentBranch.Name}" : "Branch:";
    }

    private void UpdateRemoteBranchesTreeView()
    {
        var remoteBranchesTreeViewItem = this.FindControl<TreeViewItem>("RemoteBranchesTreeViewItem");
        if (remoteBranchesTreeViewItem == null)
            return;
        remoteBranchesTreeViewItem.Items.Clear();
        foreach (var branch in CurrentRepository.RemoteBranches)
        {
            var newItem = new TreeViewItem
            {
                Header = branch.Name,
                IsSelected = false
            };

            newItem.DoubleTapped += async (sender, e) =>
            {
                await OnRemoteBranchDoubleClicked(branch.Name);
                e.Handled = true;
            };

            remoteBranchesTreeViewItem.Items.Add(newItem);
        }
    }

    private async Task OnRemoteBranchDoubleClicked(string name)
    {
        try
        {
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.FetchAsync(CurrentRepository.Path, currentUserProfile);
            await SyncRepository();
            await _gitRepositoryService.CreateNewLocalBranchFromRemoteAsync(CurrentRepository.Path, name);
            var splitedName = name.Split('/').ToList();
            splitedName.RemoveAt(0);
            var localBranchName = string.Join('/', splitedName);
            await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, localBranchName);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
            UpdateLocalBranchesTreeView();
            UpdateCurrentBranchNameDisplay();
            AddSuccessCard($"Created and switched to branch: {localBranchName} successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private void UpdateLocalBranchesTreeView()
    {
        var localBranchesTreeViewItem = this.FindControl<TreeViewItem>("LocalBranchesTreeViewItem");
        if (localBranchesTreeViewItem == null)
            return;
        localBranchesTreeViewItem.Items.Clear();
        foreach (var branch in CurrentRepository.LocalBranches)
        {
            localBranchesTreeViewItem.Items.Add(GetLocalBranchViewItem(branch));
        }
    }

    private TreeViewItem GetLocalBranchViewItem(GitBranch branch)
    {
        var newItem = new TreeViewItem
        {
            IsSelected = false,
        };

        newItem.Header = GetLocalBranchStatusIndicator(branch);
        newItem.ContextFlyout = GetLocalBranchFlyoutMenu(branch);

        if (branch.IsCurrent)
            newItem.Background = new SolidColorBrush(Color.FromRgb(81, 81, 81));

        ActionOnItemIsDoubleClick(branch, newItem);

        return newItem;
    }

    private void ActionOnItemIsDoubleClick(GitBranch branch, TreeViewItem newItem)
    {
        newItem.DoubleTapped += async (sender, e) =>
        {
            await OnLocalBranchDoubleClicked(branch.Name);
            e.Handled = true;
        };
    }

    private static TextBlock GetLocalBranchStatusIndicator(GitBranch branch)
    {
        return new TextBlock
        {
            Inlines = {
                    new Run{ Text = branch.Name },
                    new Run{ Text = branch.InComming > 0 ? $"  ⇃{branch.InComming}" : "", Foreground = Brushes.Green, FontSize = 12 },
                    new Run{ Text = branch.OutGoing > 0 ? $"  ↾{branch.OutGoing}" : "", Foreground = Brushes.Orange, FontSize = 12 }
                },
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
    }

    private Flyout GetLocalBranchFlyoutMenu(GitBranch branch)
    {
        var newLocalbrancheFlyoutComponent = _serviceProvider.GetService(typeof(LocalbrancheFlyout)) as LocalbrancheFlyout;
        var flyout = new Flyout
        {
            Placement = PlacementMode.Right,
            Content = newLocalbrancheFlyoutComponent
        };
        ActionOnLocalBranchFlyoutMenuIsOpened(branch, newLocalbrancheFlyoutComponent, flyout);
        ActionOnCreateNewLocalbranchClicked(branch, newLocalbrancheFlyoutComponent, flyout);
        ActionOnMergeBranchClicked(branch, newLocalbrancheFlyoutComponent, flyout);
        ActionOnDeleteBranchClicked(branch, newLocalbrancheFlyoutComponent, flyout);
        return flyout;
    }

    private void ActionOnDeleteBranchClicked(GitBranch branch, LocalbrancheFlyout newLocalbrancheFlyoutComponent, Flyout flyout)
    {
        newLocalbrancheFlyoutComponent!.OnDeleteClicked += async (s, e) =>
        {
            try
            {
                flyout.Hide();
                await ConfirmIfToDeleteLocalBranch(branch);
            }
            catch (Exception ex)
            {
                AddErrorCard(ex.Message);
                return;
            }
        };
    }

    private async Task ConfirmIfToDeleteLocalBranch(GitBranch branch)
    {
        var newWindow = new Window
        {
            Title = "Confirm Delete Branch",
            Width = 500,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        newWindow.Content = GetConfirmDeleteBranchComponent(branch, newWindow);
        newWindow.ShowDialog(this);
    }

    private CommonConfirmation GetConfirmDeleteBranchComponent(GitBranch branch, Window newWindow)
    {
        var commonConfirmation = _serviceProvider.GetService(typeof(CommonConfirmation)) as CommonConfirmation;
        commonConfirmation.Message = $"Are you sure you want to delete the branch '{branch.Name}'?";
        commonConfirmation.OnYesClicked += async (ss, ee) =>
        {
            await ProcessDeleteBranch(branch);
            newWindow.Close();
        };
        commonConfirmation.OnCancelClicked += (ss, ee) =>
        {
            newWindow.Close();
        };
        return commonConfirmation;
    }

    private async Task ProcessDeleteBranch(GitBranch branch)
    {
        try
        {
            await _gitRepositoryService.DeleteLocalBranchAsync(CurrentRepository.Path, branch.Name);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
            UpdateLocalBranchesTreeView();
            UpdateCurrentBranchNameDisplay();
            AddSuccessCard($"Deleted branch: {branch.Name} successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private void ActionOnMergeBranchClicked(GitBranch branch, LocalbrancheFlyout newLocalbrancheFlyoutComponent, Flyout flyout)
    {
        newLocalbrancheFlyoutComponent!.OnMergeBranchClicked += async (s, e) =>
        {
            try
            {
                flyout.Hide();
                var userProfile = await _userProfileService.GetCurrentUserProfileAsync();
                await _gitRepositoryService.MergeBranchAsync(CurrentRepository, branch.Name, userProfile);
                CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
                CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(CurrentRepository.Path);
                UpdateLocalBranchesTreeView();
                UpdateCurrentBranchNameDisplay();
                UpdateCommitsInfoView();
                AddSuccessCard($"Merged branch: {branch.Name} into current branch successfully.");
            }
            catch (Exception ex)
            {
                AddErrorCard(ex.Message);
                return;
            }
        };
    }

    private void OnClickCreateBranchOnToolBar(object? sender, RoutedEventArgs e)
    {
        try
        {
            LoadCreateBranchInputWindow();
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private void ActionOnCreateNewLocalbranchClicked(GitBranch branch, LocalbrancheFlyout newLocalbrancheFlyoutComponent, Flyout flyout)
    {
        newLocalbrancheFlyoutComponent!.OnCreateBranchHereClicked += async (s, e) =>
        {
            try
            {
                flyout.Hide();
                LoadCreateBranchInputWindow();
            }
            catch (Exception ex)
            {
                AddErrorCard(ex.Message);
                return;
            }
        };
    }

    private void LoadCreateBranchInputWindow()
    {
        var newWindow = new Window
        {
            Title = "Create New Branch",
            Width = 500,
            Height = 100,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        newWindow.Content = GetBranchNameInputComponent(newWindow);
        newWindow.ShowDialog(this);
    }

    private CommonSimpleInput GetBranchNameInputComponent(Window newWindow)
    {
        var commonSimpleInput = _serviceProvider.GetService(typeof(CommonSimpleInput)) as CommonSimpleInput;
        commonSimpleInput.IsAllowSpaces = false;
        commonSimpleInput.PlaceHolderText = "Enter new branch name";
        commonSimpleInput.OnOkClicked += async (ss, ee) =>
        {
            try
            {
                await _gitRepositoryService.CreateNewLocalBranchAsync(CurrentRepository.Path, commonSimpleInput.InputText);
                CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
                UpdateLocalBranchesTreeView();
                AddSuccessCard($"Created new branch: {commonSimpleInput.InputText} successfully.");
            }
            catch (Exception ex)
            {
                AddErrorCard(ex.Message);
                return;
            }
            newWindow.Close();
        };
        commonSimpleInput.OnCancelClicked += (ss, ee) =>
        {
            newWindow.Close();
        };
        return commonSimpleInput;
    }

    private void ActionOnLocalBranchFlyoutMenuIsOpened(GitBranch branch, LocalbrancheFlyout? newLocalbrancheFlyoutComponent, Flyout flyout)
    {
        flyout.Opened += (s, e) =>
        {
            try
            {
                var flyoutLoadFrom = branch.Name;
                newLocalbrancheFlyoutComponent.UpdateFlyout(flyoutLoadFrom, CurrentRepository.LocalBranches.FirstOrDefault(b => b.IsCurrent)?.Name ?? "");
            }
            catch (Exception ed)
            {
                AddErrorCard(ed.Message);
                return;
            }
        };
    }

    private async Task OnLocalBranchDoubleClicked(string name)
    {
        try
        {
            await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, name);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
            UpdateLocalBranchesTreeView();
            UpdateCurrentBranchNameDisplay();
            AddSuccessCard($"Switched to branch: {name} successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }

    }

    private static StackPanel AddGreenDot()
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };
        stackPanel.Children.Add(new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = Brushes.Green
        });
        return stackPanel;
    }

    private async void OnFetch(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.FetchAsync(CurrentRepository.Path, currentUserProfile);
            await SyncRepository();
            AddSuccessCard("Fetched latest changes from remote repository successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private async void OnPull(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentRepository.Name))
                throw new Exception("No repository is currently loaded.");
            await _gitRepositoryService.CheckoutBranchAsync(CurrentRepository, CurrentRepository.LocalBranches.FirstOrDefault(b => b.IsCurrent)?.Name ?? "");
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.FetchAsync(CurrentRepository.Path, currentUserProfile);
            await _gitRepositoryService.PullAsync(currentUserProfile, CurrentRepository);
            await SyncRepository();
            AddSuccessCard("Pulled latest changes from remote repository successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }

    private async void OnPush(object sender, RoutedEventArgs e)
    {
        try
        {
            var userProfile = await _userProfileService.GetCurrentUserProfileAsync();
            await _gitRepositoryService.PushAsync(CurrentRepository.Path, CurrentRepository, userProfile);
            await SyncRepository();
            AddSuccessCard("Changes pushed to remote repository successfully.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("that you are trying to push does not track an upstream branch."))
            {
                ConfirmToPushToOrigin();
                return;
            }
            AddErrorCard(ex.Message);
            return;
        }
    }

    private void ConfirmToPushToOrigin()
    {
        var newWindow = new Window
        {
            Title = "Confirm Push to Origin",
            Width = 500,
            Height = double.NaN,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        newWindow.Content = GetConfirmPushToOriginComponent(newWindow);
        newWindow.ShowDialog(this);
    }

    private object? GetConfirmPushToOriginComponent(Window newWindow)
    {
        var commonConfirmation = _serviceProvider.GetService(typeof(CommonConfirmation)) as CommonConfirmation;
        commonConfirmation!.Message = $"Do you want to publish branch?";
        commonConfirmation.OnYesClicked += async (ss, ee) =>
        {
            try
            {
                newWindow.Close();
                var userProfile = await _userProfileService.GetCurrentUserProfileAsync();
                await _gitRepositoryService.PublishBranchAsync(CurrentRepository.Path, CurrentRepository, userProfile);
                await SyncRepository();
                AddSuccessCard("Branch published and changes pushed to remote repository successfully.");
            }
            catch (Exception ex)
            {
                AddErrorCard(ex.Message);
                return;
            }
            newWindow.Close();
        };
        commonConfirmation.OnCancelClicked += (ss, ee) =>
        {
            newWindow.Close();
        };
        return commonConfirmation;
    }


    private void OnCreateBranch(object sender, RoutedEventArgs e)
    {
        // TODO: Implement create branch dialog
    }

    private async void OnRefresh(object sender, RoutedEventArgs e)
    {
        try
        {
            await SyncRepository();
            AddSuccessCard("Refresh successfully");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }
    private async void OnCommitButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (CurrentRepository.CommitDetail != null && CurrentRepository.CommitDetail.Commit.IsWorkInProgress)
            {
                var commitMessageTextBox = this.FindControl<TextBox>("CommitMessageTextBox");
                var commitDescriptionTextBox = this.FindControl<TextBox>("CommitDescriptionTextBox");
                if (commitMessageTextBox == null || commitDescriptionTextBox == null)
                {
                    return;
                }
                await _gitRepositoryService.ValidateIfCommitIsPossibleAsync(CurrentRepository.Path, CurrentRepository);
                var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
                var commitRequest = await _gitRepositoryService.GetCommitRequestAsync(CurrentRepository.Path, CurrentRepository, commitMessageTextBox.Text, commitDescriptionTextBox.Text, currentUserProfile);
                await _gitRepositoryService.CommitAsync(CurrentRepository.Path, CurrentRepository, commitRequest);
                CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(CurrentRepository.Path);
                CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(CurrentRepository.Path);
                CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(CurrentRepository.Path);
                UpdateCurrentRepositoryDisplay();
                await ShowCommitDetails(CurrentRepository.CommitDetail.Commit);
                AddSuccessCard("Changes committed successfully.");
            }
            else
            {
                AddErrorCard("No changes to commit.");
            }
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
        }
    }
    private void OnOpenProfile(object sender, RoutedEventArgs e)
    {
        var profileWindow = _serviceProvider.GetService(typeof(ProfileManagementWindow)) as ProfileManagementWindow;
        if (profileWindow != null)
        {
            profileWindow.ShowDialog(this);
            profileWindow.Closed += async (s, args) =>
            {
                await LoadProfileInfoDisplay();
            };
        }
    }

    private void OnCloneRepository(object sender, RoutedEventArgs e)
    {
        var newWindow = new Window
        {
            Title = "Clone Repository",
            Width = 500,
            Height = 100,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        var commontInput = new CommonSimpleInput();
        commontInput.PlaceHolderText = "Enter repository clone URL";
        newWindow.Content = commontInput;
        commontInput.OnCancelClicked += (ss, ee) =>
        {
            newWindow.Close();
        };
        commontInput.OnOkClicked += async (ss, ee) =>
        {
            newWindow.Close();
            await CloneRepository(commontInput.InputText);
        };
        newWindow.ShowDialog(this);
    }

    private async Task CloneRepository(string url)
    {
        try
        {
            _gitRepositoryService.ValidateGitRepositoryUrl(url);
            var currentUserProfile = await _userProfileService.GetCurrentUserProfileAsync();
            var localPath = await _folderPicker.ShowDialogAsync();
            if (string.IsNullOrEmpty(localPath))
                throw new Exception("No local path selected for cloning.");
            localPath = await _gitRepositoryService.CloneRepositoryAsync(url, localPath, currentUserProfile);
            CurrentRepository = await _gitRepositoryService.LoadRepositoryAsync(localPath);
            CurrentRepository.RemoteBranches = await _gitRepositoryService.GetRemoteBranchesAsync(localPath);
            CurrentRepository.LocalBranches = await _gitRepositoryService.GetLocalBranchesAsync(localPath);
            CurrentRepository.Commits = await _gitRepositoryService.GetCommitsAsync(localPath);
            UpdateCurrentRepositoryDisplay();
            _appStageService.SaveCurrentRepository(CurrentRepository);
            InitializeGitWatcher();
            OnRepositoryOpenedSuccessfully?.Invoke(this, EventArgs.Empty);
            AddSuccessCard($"Repository cloned to: {localPath} successfully.");
        }
        catch (Exception ex)
        {
            AddErrorCard(ex.Message);
            return;
        }
    }
}