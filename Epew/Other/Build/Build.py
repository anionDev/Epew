import sys
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def build():
    t = TasksForCommonProjectStructure()
    t.standardized_tasks_build_for_dotnet_library_project(
        str(Path(__file__).absolute()), "QualityCheck", t.get_default_target_environmenttype_mapping(), ["win-x64"], 1, sys.argv)


if __name__ == "__main__":
    build()
