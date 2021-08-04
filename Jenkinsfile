node {
   stage('QS.Libs') {
      checkout([
         $class: 'GitSCM',
         branches: scm.branches,
         doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations,
         extensions: scm.extensions + [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'QSProjects']],
         userRemoteConfigs: scm.userRemoteConfigs
      ])
      sh 'nuget restore QSProjects/QSProjectsLib.sln'
   }
   stage('Gtk.DataBindings') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Gtk.DataBindings']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/Gtk.DataBindings.git']]]
   }
   stage('GammaBinding') {
      checkout changelog: true, poll: true, scm: [$class: 'GitSCM', branches: [[name: '*/release/1.5']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'GammaBinding']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/GammaBinding.git']]]
   }
   stage('My-FyiReporting') {
      checkout changelog: true, poll: true, scm: [$class: 'GitSCM', branches: [[name: '*/QSBuild']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]]
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
   }
   stage('Test dotnet')
   {
   	sh 'dotnet test QSProjects/QSProjects.dotnet.sln'
   }
   stage('Build Net4.x') {
        sh 'msbuild /p:Configuration=Debug /p:Platform=x86 QSProjects/QSProjectsLib.sln'
        recordIssues enabledForFailure: true, tool: msBuild()
        fileOperations([fileDeleteOperation(excludes: '', includes: 'QS.Libs_linux.zip')])
        zip zipFile: 'QS.Libs_linux.zip', archive: false, dir: 'QSProjects/QS.LibsTest/bin/Debug'
        archiveArtifacts artifacts: 'QS.Libs_linux.zip', onlyIfSuccessful: true
   }
    stage('Test net4.x'){
       try {
            def PACKAGES_LOCATION = "${JENKINS_HOME}/.nuget/packages"
            sh "xvfb-run mono ${PACKAGES_LOCATION}/nunit.consolerunner/3.12.0/tools/nunit3-console.exe QSProjects/QS.LibsTest/bin/Debug/QS.LibsTest.dll"
       } catch (e) {}
       finally{
           nunit testResultsPattern: 'TestResult.xml'
       }
   }
}
