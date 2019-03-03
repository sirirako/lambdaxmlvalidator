# Setup the project
1. From Visual Studio, creat a new project.
2. Command line

```
git init
git config --global credential.helper '!aws --profile sandboxuser1 codecommit credential-helper $@'   
git config --global credential.UseHttpPath true
```
