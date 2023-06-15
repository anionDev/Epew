import sys
import os
import tempfile
import uuid
import math
import shutil
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def create_deb_package(sc: ScriptCollectionCore, codeunit_name: str, binary_folder: str, control_file_content: str,
                       deb_output_folder: str, verbosity: int, permission_of_executable_file_as_octet_triple: int) -> None:

    # prepare
    GeneralUtilities.ensure_directory_exists(deb_output_folder)
    toolname = codeunit_name
    temp_folder = os.path.join(tempfile.gettempdir(), str(uuid.uuid4()))
    GeneralUtilities.ensure_directory_exists(temp_folder)
    bin_folder = binary_folder
    tool_content_folder_name = toolname+"Content"

    # create folder
    GeneralUtilities.ensure_directory_exists(temp_folder)
    control_content_folder_name = "controlcontent"
    packagecontent_control_folder = os.path.join(temp_folder, control_content_folder_name)
    GeneralUtilities.ensure_directory_exists(packagecontent_control_folder)
    data_content_folder_name = "datacontent"
    packagecontent_data_folder = os.path.join(temp_folder, data_content_folder_name)
    GeneralUtilities.ensure_directory_exists(packagecontent_data_folder)
    entireresult_content_folder_name = "entireresultcontent"
    packagecontent_entireresult_folder = os.path.join(temp_folder, entireresult_content_folder_name)
    GeneralUtilities.ensure_directory_exists(packagecontent_entireresult_folder)

    # create "debian-binary"-file
    debianbinary_file = os.path.join(packagecontent_entireresult_folder, "debian-binary")
    GeneralUtilities.ensure_file_exists(debianbinary_file)
    GeneralUtilities.write_text_to_file(debianbinary_file, "2.0\n")

    # create control-content

    #  conffiles
    conffiles_file = os.path.join(packagecontent_control_folder, "conffiles")
    GeneralUtilities.ensure_file_exists(conffiles_file)

    #  postinst-script
    postinst_file = os.path.join(packagecontent_control_folder, f"postinst")
    GeneralUtilities.ensure_file_exists(postinst_file)
    exe_file = f"/usr/bin/{tool_content_folder_name}/{toolname}"
    link_file = f"/usr/bin/{toolname.lower()}"
    permission = str(permission_of_executable_file_as_octet_triple)
    GeneralUtilities.write_text_to_file(postinst_file, f"""#!/bin/sh
ln -s {exe_file} {link_file}
chmod {permission} {exe_file}
chmod {permission} {link_file}
""")

    #  control
    control_file = os.path.join(packagecontent_control_folder, "control")
    GeneralUtilities.ensure_file_exists(control_file)
    GeneralUtilities.write_text_to_file(control_file, control_file_content)

    #  md5sums
    md5sums_file = os.path.join(packagecontent_control_folder, "md5sums")
    GeneralUtilities.ensure_file_exists(md5sums_file)

    # create data-content

    #  copy binaries
    usr_bin_folder = os.path.join(packagecontent_data_folder, "usr/bin")
    GeneralUtilities.ensure_directory_exists(usr_bin_folder)
    usr_bin_content_folder = os.path.join(usr_bin_folder, tool_content_folder_name)
    GeneralUtilities.copy_content_of_folder(bin_folder, usr_bin_content_folder)

    # create debfile
    deb_filename = f"{toolname}.deb"
    sc.run_program_argsasarray("tar", ["czf", f"../{entireresult_content_folder_name}/control.tar.gz", "*"], packagecontent_control_folder, verbosity=verbosity)
    sc.run_program_argsasarray("tar", ["czf", f"../{entireresult_content_folder_name}/data.tar.gz", "*"], packagecontent_data_folder, verbosity=verbosity)
    sc.run_program_argsasarray("ar", ["r", deb_filename, "debian-binary", "control.tar.gz", "data.tar.gz"], packagecontent_entireresult_folder, verbosity=verbosity)
    result_file = os.path.join(packagecontent_entireresult_folder, deb_filename)
    shutil.copy(result_file, os.path.join(deb_output_folder, deb_filename))

    # cleanup
    GeneralUtilities.ensure_directory_does_not_exist(temp_folder)

# -------------------------------
# General utlities


def replace_variable_in_string(input_string: str, variable_name: str, variable_value: str) -> None:
    return input_string.replace(f"__[{variable_name}]__", variable_value)


# -------------------------------

def load_deb_control_file_content(t: TasksForCommonProjectStructure, file: str,
                                  codeunitname: str, codeunitversion: str, installedsize: int,
                                  maintainername: str, maintaineremail: str, description: str,) -> str:
    content = GeneralUtilities.read_text_from_file(file)
    content = replace_variable_in_string(content, "codeunitname", codeunitname)
    content = replace_variable_in_string(content, "codeunitversion", codeunitversion)
    content = replace_variable_in_string(content, "installedsize", str(installedsize))
    content = replace_variable_in_string(content, "maintainername", maintainername)
    content = replace_variable_in_string(content, "maintaineremail", maintaineremail)
    content = replace_variable_in_string(content, "description", description)
    return content


def calculate_deb_package_size(t: TasksForCommonProjectStructure, binary_folder: str) -> int:
    size_in_bytes = 0
    for file in GeneralUtilities.get_all_files_of_folder(binary_folder):
        size_in_bytes = size_in_bytes+os.path.getsize(file)
    result = math.ceil(size_in_bytes/1024)
    return result


def create_deb_package_for_artifact(t: TasksForCommonProjectStructure, codeunit_folder: str,
                                    maintainername: str, maintaineremail: str, description: str,
                                    verbosity: int, cmd_arguments: list[str]) -> None:
    verbosity = t.get_verbosity_from_commandline_arguments(cmd_arguments, verbosity)
    codeunit_name = os.path.basename(codeunit_folder)
    binary_folder = GeneralUtilities.resolve_relative_path("Other/Artifacts/BuildResult_DotNet_linux-x64", codeunit_folder)
    deb_output_folder = GeneralUtilities.resolve_relative_path("Other/Artifacts/BuildResult_Deb", codeunit_folder)
    control_file = GeneralUtilities.resolve_relative_path("Other/Build/DebControlFile.txt", codeunit_folder)
    installedsize = calculate_deb_package_size(t, binary_folder)
    control_file_content = load_deb_control_file_content(t, control_file, codeunit_name, t.get_version_of_codeunit_folder(codeunit_folder),
                                                         installedsize, maintainername, maintaineremail, description)
    create_deb_package(ScriptCollectionCore(), codeunit_name, binary_folder, control_file_content, deb_output_folder, verbosity, 555)


def build():
    verbosity = 1
    this_file = str(Path(__file__).absolute())
    t = TasksForCommonProjectStructure()
    codeunit_folder = GeneralUtilities.resolve_relative_path("../../..", this_file)
    t.standardized_tasks_build_for_dotnet_project(this_file, "QualityCheck", t.get_default_target_environmenttype_mapping(), ["win-x64", "linux-x64"], verbosity, sys.argv)
    create_deb_package_for_artifact(t, codeunit_folder,
                                    t.get_constant_value(codeunit_folder, "maintainername"),
                                    t.get_constant_value(codeunit_folder, "maintaineremail"),
                                    t.get_constant_value(codeunit_folder, "description"),
                                    verbosity, sys.argv)


if __name__ == "__main__":
    build()
