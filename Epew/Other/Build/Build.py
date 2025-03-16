import sys
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def build():
    verbosity = 1
    this_file = str(Path(__file__).absolute())
    t = TasksForCommonProjectStructure()
    codeunit_folder = GeneralUtilities.resolve_relative_path("../../..", this_file)
    t.standardized_tasks_build_for_dotnet_project(this_file, "QualityCheck", t.get_default_target_environmenttype_mapping(), ["win-x64", "linux-x64"], verbosity, sys.argv)
    t.create_deb_package_for_artifact(codeunit_folder, t.get_constant_value(codeunit_folder, "MaintainerName"), t.get_constant_value(codeunit_folder, "MaintainerEMailAddress"), t.get_constant_value(codeunit_folder, "CodeUnitDescription"), verbosity, sys.argv)
    t.create_zip_file_for_artifact(codeunit_folder, "BuildResult_DotNet_win-x64", "Epew-Zip-for-Windows", verbosity, sys.argv)


if __name__ == "__main__":
    build()
