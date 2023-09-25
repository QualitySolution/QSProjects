properties([parameters([
	booleanParam(defaultValue: false, description: 'Старый плагин отображения покрытия', name: 'OldCoverage')
])])
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
   stage('My-FyiReporting') {
      checkout changelog: true, poll: true, scm: [$class: 'GitSCM', branches: [[name: '*/QSBuild']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]]
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
   }
   stage('Test dotnet')
   {
   	sh 'rm -rf QSProjects/QS.LibsTest.Core/TestResults'
      try {  
   	  sh 'dotnet test --logger trx --collect:"XPlat Code Coverage" QSProjects/QSProjects.dotnet.sln'
      } catch (e) {}
      finally{
   	  if (params.OldCoverage) {
   	    cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/TestResults/**/coverage.cobertura.xml', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, zoomCoverageChart: false
   	  }
   	  else {
   	    publishCoverage adapters: [coberturaAdapter(mergeToOneReport: true, path: '**/TestResults/**/coverage.cobertura.xml')], checksName: '', sourceFileResolver: sourceFiles('STORE_LAST_BUILD')
   	  }
   	  mstest testResultsFile:"**/*.trx", keepLongStdio: true
      }
   }
   stage('Build Net4.x') {
        sh 'msbuild /p:Configuration=Debug /p:Platform=x86 QSProjects/QSProjectsLib.sln'
        fileOperations([fileDeleteOperation(excludes: '', includes: 'QS.Libs_linux.zip')])
        zip zipFile: 'QS.Libs_linux.zip', archive: false, dir: 'QSProjects/QS.LibsTest/bin/Debug'
        archiveArtifacts artifacts: 'QS.Libs_linux.zip', onlyIfSuccessful: true
   }
    stage('Test net4.x'){
       try {
            def PACKAGES_LOCATION = "${JENKINS_HOME}/.nuget/packages"
            sh "xvfb-run mono ${PACKAGES_LOCATION}/nunit.consolerunner/3.16.3/tools/nunit3-console.exe QSProjects/QS.LibsTest/bin/Debug/QS.LibsTest.dll --framework=mono-4.0"
       } catch (e) {}
       finally{
           nunit testResultsPattern: 'TestResult.xml'
       }
   }
}
