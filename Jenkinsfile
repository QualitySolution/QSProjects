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
   	  discoverGitReferenceBuild()
        recordCoverage ignoreParsingErrors: true, tools: [[parser: 'COBERTURA', pattern: '**/coverage.cobertura.xml']]
        mstest testResultsFile:"**/*.trx", keepLongStdio: true
      }
   }
   stage('Build Net4.x') {
        sh 'msbuild /p:Configuration=Debug /p:Platform=x86 QSProjects/QSProjectsLib.sln'
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
