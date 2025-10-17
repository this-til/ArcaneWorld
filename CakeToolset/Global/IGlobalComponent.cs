using Godot;

namespace CakeToolset.Global;

public interface IGlobalComponent {

    void initialize();

    void terminate();

    int priority => 0;

    Node asNode => (Node)this;

}
