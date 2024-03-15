<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { onMount } from 'svelte';
    import {fetchChats} from '$lib/Handlers/groupHandler'
    import {getToken} from '$lib/Handlers/authHandler'
</script>
    

<div class="app">
    <nav class="navbar navbar-expand-lg navbar-light">
        <a class="navbar-brand" href="/">Boomerangr</a>
    </nav>  
    <main>
        <slot/>
    </main>
</div>


<style>
    .app {
        box-sizing: border-box;
        margin:0;
        padding:0;
        display: flex;
        flex-direction: column;
        height: 100vh;
        width:100vw;
    }
        main {
            flex-grow: 1;
            display: flex;
            flex-direction: row;
        }
            nav{
                background-color: var(--prim);
            }
            a{
                color: var(--prim-fg) !important;
                margin-left: 2rem;
            }
</style>






