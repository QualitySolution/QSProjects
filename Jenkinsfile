node {
   stage('QS.Libs') {
      echo sh(script: 'env|sort', returnStdout: true)
      if (env.CHANGE_ID) {
          branch = '*/merge';
          ref = '+refs/pull/${CHANGE_ID}/*:refs/remotes/origin/pr/${CHANGE_ID}/*'
      } else {
          branch = '${BRANCH_NAME}';
          ref = '+refs/heads/*:refs/remotes/origin/*'
      }
      checkout([$class: 'GitSCM', branches: [[name: branch]], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'QSProjects']], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'faa4f01a-3033-4806-a4cf-2eac0ecdb522', name: 'origin', refspec: ref, url: 'https://github.com/QualitySolution/QSProjects.git']]])
      sh 'nuget restore QSProjects/QSProjectsLib.sln'
   }
   stage('Gtk.DataBindings') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Gtk.DataBindings']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/Gtk.DataBindings.git']]]
   }
   stage('GammaBinding') {
      checkout changelog: true, poll: true, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'GammaBinding']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/GammaBinding.git']]]
   }
   stage('My-FyiReporting') {
      checkout changelog: true, poll: true, scm: [$class: 'GitSCM', branches: [[name: '*/QSBuild']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]]
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
   }
   stage('Build') {
        sh 'msbuild /p:Configuration=Debug /p:Platform=x86 QSProjects/QSProjectsLib.sln'
        recordIssues enabledForFailure: true, tool: msBuild()
        fileOperations([fileDeleteOperation(excludes: '', includes: 'QS.Libs_linux.zip')])
        zip zipFile: 'QS.Libs_linux.zip', archive: false, dir: 'QSProjects/QS.LibsTest/bin/Debug'
        archiveArtifacts artifacts: 'QS.Libs_linux.zip', onlyIfSuccessful: true
   }
    stage('Test'){
       try {
            sh 'xvfb-run mono QSProjects/packages/NUnit.ConsoleRunner.3.10.0/tools/nunit3-console.exe QSProjects/QS.LibsTest/bin/Debug/QS.LibsTest.dll'
       } catch (e) {}
       finally{
           nunit testResultsPattern: 'TestResult.xml'
       }
   }
}
