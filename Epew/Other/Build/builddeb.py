import os
import sys
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def create_deb_package(sc: ScriptCollectionCore, toolname: str, version: str, maintainer_name: str, maintainer_email: str, description: str) -> None:
    base_folder = "C:\\Users\\User\\Desktop\\deb-test"
    bin_folder = os.path.join(base_folder, "bin")
    tool_content_folder_name = toolname+"Content"

    # create folder
    temp_folder = os.path.join(base_folder, "temp")
    GeneralUtilities.ensure_directory_does_not_exist(temp_folder)
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
    GeneralUtilities.write_text_to_file(postinst_file, f"""#!/bin/sh
ln -s {exe_file} {link_file}
chmod +x {exe_file}
chmod +x {link_file}
""")

    #  control
    size = 1234  # TODO calculate size of package in KB
    control_file = os.path.join(packagecontent_control_folder, "control")
    GeneralUtilities.ensure_file_exists(control_file)
    GeneralUtilities.write_text_to_file(control_file, f"""Package: {toolname}
Version: {version}
Section: text
Priority: optional
Architecture: all
Depends:
Recommends: top
Installed-Size: {size}
Maintainer: {maintainer_name} <{maintainer_email}>
Description: {description}
""")

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
    sc.run_program_argsasarray("tar", ["czf", f"../{entireresult_content_folder_name}/control.tar.gz", "*"], packagecontent_control_folder)
    sc.run_program_argsasarray("tar", ["czf", f"../{entireresult_content_folder_name}/data.tar.gz", "*"], packagecontent_data_folder)

    # cleanup
    sc.run_program_argsasarray("ar", ["r", f"{toolname}.deb", "debian-binary", "control.tar.gz", "data.tar.gz"], packagecontent_entireresult_folder)
    # GeneralUtilities.ensure_directory_does_not_exist(temp_folder)


def create_deb_package_for_artifact(t: TasksForCommonProjectStructure, build_script_file: str) -> None:
    create_deb_package(ScriptCollectionCore(), "Epew", "5.0.11", "Marius Göcke", "marius.goecke@gmail.com",
                       "Epew (ExternalProgramExecutionWrapper) is a tool to wrap program-calls with some useful functions.")


def build():
    create_deb_package_for_artifact(None, None)


build()
