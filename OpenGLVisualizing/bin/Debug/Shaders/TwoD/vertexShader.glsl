#version 330 core
layout (location = 0) in vec3 aPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;

void main()
{   
    FragPos = vec3(mat4(1.0f) * vec4(aPosition, 1.0));
    gl_Position = projection * view * vec4(FragPos, 1.0);
}