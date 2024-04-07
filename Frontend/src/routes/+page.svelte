<script lang="ts">
    import { goto } from "$app/navigation";
    import { isLoggedIn } from "$lib/Handlers/accountHandler";
    import { onMount } from "svelte";

    onMount(() => {
        goto("/welcome");
    });
</script>
