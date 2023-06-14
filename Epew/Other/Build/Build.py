import sys
import os
import tempfile
import uuid
import math
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def create_deb_package(sc: ScriptCollectionCore, binary_folder: str, control_file_content: str, deb_output_folder: str) -> None:
    # TODO
    GeneralUtilities.ensure_directory_exists(deb_output_folder)

# -------------------------------
# General utlities


def replace_variable_in_string(input_string: str, variable_name: str, variable_value: str) -> None:
    return input_string.replace(f"__[{variable_name}]__", variable_value)


def load_deb_control_file_content(file: str,
                                  codeunitname: str, codeunitversion: str, installedsize: str,
                                  maintainername: str, maintaineremail: str, description: str,) -> str:
    content = GeneralUtilities.read_text_from_file(file)
    content = replace_variable_in_string(content, "codeunitname", codeunitname)
    content = replace_variable_in_string(content, "codeunitversion", codeunitversion)
    content = replace_variable_in_string(content, "installedsize", installedsize)
    content = replace_variable_in_string(content, "maintainername", maintainername)
    content = replace_variable_in_string(content, "maintaineremail", maintaineremail)
    content = replace_variable_in_string(content, "description", description)
    return content
# -------------------------------


def calculate_deb_package_size(binary_folder: str) -> int:
    size_in_bytes = 0
    for file in GeneralUtilities.get_all_files_of_folder(binary_folder):
        size_in_bytes = size_in_bytes+os.path.getsize(file)
    result = math.ceil(size_in_bytes/1024)
    return result


def create_deb_package_for_artifact(t: TasksForCommonProjectStructure,
                                    codeunit_folder: str, maintainername: str, maintaineremail: str, description: str,
                                    verbosity: int, cmd_arguments: list[str]) -> None:
    codeunit_name = os.path.basename(codeunit_folder)
    binary_folder = GeneralUtilities.resolve_relative_path("Other/Artifacts/BuildResult_DotNet_linux-x64", codeunit_folder)
    temp_folder = os.path.join(tempfile.gettempdir(), str(uuid.uuid4()))
    GeneralUtilities.ensure_directory_exists(temp_folder)
    deb_output_folder = GeneralUtilities.resolve_relative_path("Other/Artifacts/BuildResult_Deb", codeunit_folder)
    control_file = GeneralUtilities.resolve_relative_path("Other/Build/DebControlFile.txt", codeunit_folder)
    installedsize = calculate_deb_package_size(binary_folder)
    control_file_content = load_deb_control_file_content(control_file, codeunit_name, t.get_version_of_codeunit_folder(codeunit_folder),
                                                         installedsize, maintainername, maintaineremail, description)
    create_deb_package(ScriptCollectionCore(), binary_folder, control_file_content, deb_output_folder)


def build():
    verbosity = 1
    this_file = str(Path(__file__).absolute())
    t = TasksForCommonProjectStructure()
    codeunit_folder = GeneralUtilities.resolve_relative_path("../../..", this_file)
    # t.standardized_tasks_build_for_dotnet_project(this_file, "QualityCheck", t.get_default_target_environmenttype_mapping(), ["win-x64", "linux-x64"], verbosity, sys.argv)
    create_deb_package_for_artifact(t, this_file,
                                    t.get_constant_value(codeunit_folder, "maintainername"),
                                    t.get_constant_value(codeunit_folder, "maintaineremail"),
                                    t.get_constant_value(codeunit_folder, "description"),
                                    verbosity, sys.argv)


if __name__ == "__main__":
    build()
