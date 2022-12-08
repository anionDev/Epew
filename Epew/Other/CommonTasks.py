import sys
import os
from pathlib import Path
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def common_tasks():
    file = str(Path(__file__).absolute())
    folder_of_current_file = os.path.dirname(file)
    sc = ScriptCollectionCore()
    version = sc.get_semver_version_from_gitversion(GeneralUtilities.resolve_relative_path("../..", os.path.dirname(file)))
    sc.replace_version_in_csproj_file(GeneralUtilities.resolve_relative_path("../Epew/Epew.csproj", folder_of_current_file), version)
    sc.replace_version_in_csproj_file(GeneralUtilities.resolve_relative_path("../EpewTests/EpewTests.csproj", folder_of_current_file), version)
    t = TasksForCommonProjectStructure()
    t.standardized_tasks_do_common_tasks(file, version, 1, "QualityCheck", True, None, sys.argv)


if __name__ == "__main__":
    common_tasks()
