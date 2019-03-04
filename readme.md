# .Net Core serverless Deployment
1. From Visual Studio, creat a new project.
2. Command line

```
git init
git config --local credential.helper '!aws --profile sandboxuser1 codecommit credential-helper $@'   
git config --local credential.UseHttpPath true
```
3. Create CodeCommit repository and add it the project.

```
git commit -m "initial commit"
git config --local store
git push --set-upstream xxxx  master   --force


```

4. Add serverless template.
3. Package and deploy to create CFN stack.
```
dotnet publish -c release
sam package --s3-bucket siri-sandbox-build --output-template xmlchecker-updated-serverless.template --template-file .\serverless.yml --profile sandboxuser1 --region us-east-1
sam deploy --template-file ./xmlchecker-updated-serverless.template --stack-name xmlchecker --capabilities CAPABILITY_IAM

dotnet lambda package-ci --s3-bucket siri-sandbox-build --output-template xmlchecker-updated-serverless.template --template serverless.yml  --profile sandboxuser1 --region us-east-1
```
## Deploy serverless using CloudFormation Changeset using CodePipeline and CodeBuild

1. Add serverless template.  The different is CodeURI need to be empty.
2. Add buildspec.yml
3. Configure CodeBuild project
4. Configure CodePipeline with CodeCommit, CodeBuild and Cloudformation Create Changeset and Execute Changeset.

## Deploy serverless project using CodeDeploy

