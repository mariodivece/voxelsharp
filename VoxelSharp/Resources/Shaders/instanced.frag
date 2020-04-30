#version 330 core
out vec4 FragColor;
out vec3 viewDir;
uniform vec3 viewPos;

void main()
{
    viewDir = normalize(viewPos);
    FragColor = vec4(0.0, 1.0, 0.0, 1.0);
}