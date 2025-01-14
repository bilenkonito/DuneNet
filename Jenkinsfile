pipeline {
    agent any
    environment {
        DEV_VERSION = "${env.BRANCH_NAME}-SNAPSHOT"
		RELEASE_VERSION = "release-${currentBuild.number}"
    }
    stages {
        stage ('Build Development') {
            when {
              expression {
                env.BRANCH_NAME != null && env.BRANCH_NAME != 'production'
              }
            }
            steps {
                    withMaven(maven: 'Maven 3.5.3', mavenSettingsConfig: 'e6e19d20-925f-4581-be31-fadc65d0f8dc') {
						bat "mvn clean install deploy -DartifactVersion=${DEV_VERSION}"
					}
            }
        }
    }
}