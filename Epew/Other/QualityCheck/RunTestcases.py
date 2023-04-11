import sys
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def run_testcases():
    t = TasksForCommonProjectStructure()
    t.standardized_tasks_run_testcases_for_dotnet_project(str(Path(__file__).absolute()), "QualityCheck", 1, True, t.get_default_target_environmenttype_mapping(), sys.argv)


if __name__ == "__main__":
    run_testcases()
