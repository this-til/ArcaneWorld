namespace ArcaneWorld.Generated;

public static class ResourceTree {

    public const string ArcaneWorld_csproj = "ArcaneWorld.csproj";
    public const string assembly_load_config_json = "assembly_load.config.json";
    public const string icon_svg = "icon.svg";
    public const string icon_svg_import = "icon.svg.import";
    public const string main_tscn = "main.tscn";
    public const string project_godot = "project.godot";

    public static class Addons {

        public static class CustomType {
            public const string ClassNameAttribute_cs = "addons/custom_type/ClassNameAttribute.cs";
            public const string plugin_cfg = "addons/custom_type/plugin.cfg";
            public const string Plugin_cs = "addons/custom_type/Plugin.cs";
            public const string README_md = "addons/custom_type/README.md";
        }

        public static class ResourceTreeGenerator {
            public const string plugin_cfg = "addons/resource_tree_generator/plugin.cfg";
            public const string ResourceTreeGenerator_cs = "addons/resource_tree_generator/ResourceTreeGenerator.cs";
        }
    }

    public static class Script {
        public const string Planet_cs = "Script/Planet.cs";
        public const string PlanetBlock_cs = "Script/PlanetBlock.cs";

        public static class Attribute {
            public const string EventBusSubscriberAttribute_cs = "Script/Attribute/EventBusSubscriberAttribute.cs";
            public const string JsonConverterAutomaticLoadAttribute_cs = "Script/Attribute/JsonConverterAutomaticLoadAttribute.cs";
            public const string SaveField_cs = "Script/Attribute/SaveField.cs";
        }

        public static class Capacity {
            public const string IContainer_cs = "Script/Capacity/IContainer.cs";
            public const string IHandler_cs = "Script/Capacity/IHandler.cs";
            public const string ILock_cs = "Script/Capacity/ILock.cs";
            public const string ISerialize_cs = "Script/Capacity/ISerialize.cs";

            public static class Instance {
                public const string Container_cs = "Script/Capacity/Instance/Container.cs";
            }
        }

        public static class Generated {
            public const string ResourceTree_cs = "Script/Generated/ResourceTree.cs";
        }

        public static class Global {
            public const string AssemblyLoadManage_cs = "Script/Global/AssemblyLoadManage.cs";
            public const string EventBusHold_cs = "Script/Global/EventBusHold.cs";
            public const string JsonSerializerHold_cs = "Script/Global/JsonSerializerHold.cs";
            public const string LogManage_cs = "Script/Global/LogManage.cs";
            public const string RegisterSystemHold_cs = "Script/Global/RegisterSystemHold.cs";
        }

        public static class Mechanical {
            public const string ActuatorMechanicalBase_cs = "Script/Mechanical/ActuatorMechanicalBase.cs";
            public const string MechanicalBase_cs = "Script/Mechanical/MechanicalBase.cs";
        }

        public static class Register {
            public const string DustMaterial_cs = "Script/Register/DustMaterial.cs";
            public const string Fluid_cs = "Script/Register/Fluid.cs";
            public const string GemstoneMaterial_cs = "Script/Register/GemstoneMaterial.cs";
            public const string Item_cs = "Script/Register/Item.cs";
            public const string Material_cs = "Script/Register/Material.cs";
            public const string MetalMaterial_cs = "Script/Register/MetalMaterial.cs";
            public const string OreMaterial_cs = "Script/Register/OreMaterial.cs";
            public const string RegisterPriority_cs = "Script/Register/RegisterPriority.cs";
            public const string Vis_cs = "Script/Register/Vis.cs";
        }

        public static class Serialize {
            public const string RegisterItemJsonConverter_cs = "Script/Serialize/RegisterItemJsonConverter.cs";
        }

        public static class Util {
            public const string GodotAppender_cs = "Script/Util/GodotAppender.cs";
            public const string ReadLockContext_cs = "Script/Util/ReadLockContext.cs";
            public const string SimpleNode_cs = "Script/Util/SimpleNode.cs";
            public const string WriteLockContext_cs = "Script/Util/WriteLockContext.cs";
        }
    }
}
