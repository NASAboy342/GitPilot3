using System;
using System.Collections.Generic;

namespace GitPilot3.Models;

public class AppStage
{
    public GitRepository? CurrentRepository { get; set; }

    public List<OpenedRepository> OpenedRepositories { get; set; } = new List<OpenedRepository>();
}
