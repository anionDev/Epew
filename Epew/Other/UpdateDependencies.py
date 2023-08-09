import sys
import os
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def update_dependencies():
    TasksForCommonProjectStructure().update_dependencies_of_typical_dotnet_codeunit(str(Path(__file__).absolute()), 1, sys.argv)


if __name__ == "__main__":
    update_dependencies()
