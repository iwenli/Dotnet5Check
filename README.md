# Dotnet5Check
一些关于net5+再docker上面部署的验证

+ 验证 `ForwardedHeaders` 中间件是否能通过 `RemoteIpAddress` 拿到正确的客户端IP
> 答案：不能，`ForwardedHeaders` 能正确转换 `X-Forwarded-For`，但是 `RemoteIpAddress` 依旧拿到的是docker网络的IP,需要自己使用`X-Forwarded-For`获取

+ 验证 `docker` 下中文验证码图片如何显示中文
> 通过在打包 `docker` 镜像时安装中文字体包来解决